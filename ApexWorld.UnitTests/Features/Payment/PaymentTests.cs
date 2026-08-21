using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Payment.Services;
using ApexWorld_Backend.Features.Payment.Models;
using ApexWorld_Backend.Features.Payment.DTOs;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Configuration;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.BackgroundJobs.Services;
using ApexWorld_Backend.Common.Resilience;
using ApexWorld_Backend.Features.Payment.Exceptions;
using ApexWorld_Backend.Common.Models;
using Microsoft.Extensions.Logging;

namespace ApexWorld.UnitTests.Features.Payment;

public class PaymentTests
{
    private readonly Mock<IRepository<PaymentRecord>> _paymentRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>> _bookingRepoMock;
    private readonly Mock<IRepository<LoanApplication>> _loanRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobsMock;
    private readonly Mock<System.Net.Http.IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IRuleEngine<PaymentInitiateRequestDto>> _ruleEngineMock;
    private readonly Mock<IDeadLetterQueueService> _dlqServiceMock;
    private readonly Mock<IBuyerNotificationService> _buyerNotificationServiceMock;
    private readonly ExponentialBackoffRetryPolicy _retryPolicy;
    private readonly PaymentService _sut;

    public PaymentTests()
    {
        _paymentRepoMock = new Mock<IRepository<PaymentRecord>>();
        _bookingRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>>();
        _loanRepoMock = new Mock<IRepository<LoanApplication>>();
        _propertyRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>>();
        _auditServiceMock = new Mock<IAuditService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _backgroundJobsMock = new Mock<IBackgroundJobClient>();
        _httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _ruleEngineMock = new Mock<IRuleEngine<PaymentInitiateRequestDto>>();
        _dlqServiceMock = new Mock<IDeadLetterQueueService>();
        _buyerNotificationServiceMock = new Mock<IBuyerNotificationService>();
        
        var loggerMock = new Mock<ILogger<ExponentialBackoffRetryPolicy>>();
        _retryPolicy = new ExponentialBackoffRetryPolicy(loggerMock.Object, 3, 100, 2.0);

        _sut = new PaymentService(
            _paymentRepoMock.Object,
            _bookingRepoMock.Object,
            _loanRepoMock.Object,
            _propertyRepoMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object,
            _backgroundJobsMock.Object,
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _ruleEngineMock.Object,
            _retryPolicy,
            _dlqServiceMock.Object,
            _buyerNotificationServiceMock.Object
        );
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenRuleEngineFails_ShouldThrowInvalidPaymentMethodException()
    {
        // Arrange
        var request = new PaymentInitiateRequestDto { PaymentMethod = "Invalid" };
        var buyerId = "1";
        
        var ruleResult = new RuleResult();
        ruleResult.AddError("Invalid payment method");
        _ruleEngineMock.Setup(r => r.Evaluate(request)).Returns(ruleResult);

        // Act
        Func<Task> act = async () => await _sut.InitiatePaymentAsync(buyerId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidPaymentMethodException>().WithMessage("*Invalid payment method*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenValidRequestAndFallbackMethod_ShouldUpdateBookingAndProperty()
    {
        // Arrange
        var request = new PaymentInitiateRequestDto { BookingId = 1, PaymentMethod = "NetBanking" };
        var buyerId = "1";

        var ruleResult = new RuleResult();
        _ruleEngineMock.Setup(r => r.Evaluate(request)).Returns(ruleResult);

        var booking = new ApexWorld_Backend.Features.Booking.Models.Booking { Id = 1, BuyerId = 1, Status = "Pending", PropertyId = 100 };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        _loanRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<LoanApplication>());

        var property = new ApexWorld_Backend.Features.Property.Models.Property { Id = 100, Status = "Available", IsAvailable = true };
        _propertyRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(property);

        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<PaymentRecord>())).ReturnsAsync(new PaymentRecord());

        // Act
        var result = await _sut.InitiatePaymentAsync(buyerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.TransactionId.Should().StartWith("Manual_");

        booking.Status.Should().Be("Paid");
        property.Status.Should().Be("Booked");
        property.IsAvailable.Should().BeFalse();

        _bookingRepoMock.Verify(r => r.UpdateAsync(booking), Times.Once);
        _propertyRepoMock.Verify(r => r.UpdateAsync(property), Times.Once);
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<PaymentRecord>()), Times.Once);
    }
    
    [Fact]
    public async Task ProcessWebhookAsync_WhenPaymentSucceeds_ShouldUpdateStatusAndLog()
    {
        // Arrange
        var transactionId = "txn_123";
        var status = "success";
        var bookingId = 1;

        var paymentRecord = new PaymentRecord { Id = 10, BookingId = bookingId, Status = "Pending" };
        _paymentRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<PaymentRecord> { paymentRecord });

        var booking = new ApexWorld_Backend.Features.Booking.Models.Booking { Id = bookingId, BuyerId = 1, Status = "PendingPayment", PropertyId = 100 };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        
        var property = new ApexWorld_Backend.Features.Property.Models.Property { Id = 100, Status = "Available", IsAvailable = true };
        _propertyRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(property);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessWebhookAsync(transactionId, status, bookingId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.TransactionId.Should().Be(transactionId);

        booking.Status.Should().Be("Paid");
        property.Status.Should().Be("Booked");
        
        _paymentRepoMock.Verify(r => r.UpdateAsync(paymentRecord), Times.Once);
        _bookingRepoMock.Verify(r => r.UpdateAsync(booking), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync("PaymentSuccess", "PaymentRecord", "10", It.IsAny<string>(), "System"), Times.Once);
    }
}
