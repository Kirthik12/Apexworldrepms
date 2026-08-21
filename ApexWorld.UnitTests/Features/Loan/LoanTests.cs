using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using ApexWorld_Backend.Features.Loan.Services;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Loan.DTOs;
using ApexWorld_Backend.Features.Loan.Exceptions;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Payment.Models;
using ApexWorld_Backend.Features.Booking.Services;
using ApexWorld_Backend.Common.Services;
using ApexWorld_Backend.Features.Notifications.Services;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using PropertyEntity = ApexWorld_Backend.Features.Property.Models.Property;

namespace ApexWorld.UnitTests.Features.Loan;

public class LoanTests
{
    private readonly Mock<IRepository<LoanApplication>> _loanRepoMock;
    private readonly Mock<IRepository<PaymentRecord>> _paymentRepoMock;
    private readonly Mock<IRepository<BookingEntity>> _bookingRepoMock;
    private readonly Mock<IBookingService> _bookingServiceMock;
    private readonly Mock<IBulkheadService> _bulkheadServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<EMIPlan>> _emiPlanRepoMock;
    private readonly Mock<IRepository<PropertyEntity>> _propertyRepoMock;
    private readonly Mock<IAdminNotificationService> _adminNotificationServiceMock;
    private readonly Mock<IBuyerNotificationService> _buyerNotificationServiceMock;
    private readonly LoanService _sut;

    public LoanTests()
    {
        _loanRepoMock = new Mock<IRepository<LoanApplication>>();
        _paymentRepoMock = new Mock<IRepository<PaymentRecord>>();
        _bookingRepoMock = new Mock<IRepository<BookingEntity>>();
        _bookingServiceMock = new Mock<IBookingService>();
        _bulkheadServiceMock = new Mock<IBulkheadService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emiPlanRepoMock = new Mock<IRepository<EMIPlan>>();
        _propertyRepoMock = new Mock<IRepository<PropertyEntity>>();
        _adminNotificationServiceMock = new Mock<IAdminNotificationService>();
        _buyerNotificationServiceMock = new Mock<IBuyerNotificationService>();

        // Setup Bulkhead to just execute the func directly
        _bulkheadServiceMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task<LoanApplication>>>()))
            .Returns<string, Func<Task<LoanApplication>>>((key, func) => func());

        _sut = new LoanService(
            _loanRepoMock.Object,
            _paymentRepoMock.Object,
            _bookingRepoMock.Object,
            _bookingServiceMock.Object,
            _bulkheadServiceMock.Object,
            _unitOfWorkMock.Object,
            _emiPlanRepoMock.Object,
            _propertyRepoMock.Object,
            _adminNotificationServiceMock.Object,
            _buyerNotificationServiceMock.Object
        );
    }

    [Fact]
    public async Task ApplyForLoanAsync_WhenPaymentExists_ShouldThrowException()
    {
        // Arrange
        var request = new LoanApplicationRequestDto
        {
            BookingId = 1,
            BuyerId = 10,
            PropertyId = 5,
            LoanAmount = 100000,
            BankName = "TestBank",
            BuyerName = "John Doe"
        };

        var payments = new List<PaymentRecord>
        {
            new PaymentRecord { BookingId = 1, Status = "Pending" }
        };
        
        _paymentRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(payments);

        // Act
        Func<Task> act = async () => await _sut.ApplyForLoanAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*payment has already been initiated*");
    }

    [Fact]
    public async Task GetBuyerLoansAsync_WhenValidBuyerId_ShouldReturnLoans()
    {
        // Arrange
        int buyerId = 10;
        var loans = new List<LoanApplication>
        {
            new LoanApplication { Id = 1, BuyerId = buyerId, Status = "Pending" },
            new LoanApplication { Id = 2, BuyerId = buyerId, Status = "Approved" }
        };

        _loanRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(loans);

        // Act
        var result = await _sut.GetBuyerLoansAsync(buyerId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.All(l => l.BuyerId == buyerId).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLoanStatusAsync_WhenStatusIsApproved_ShouldUpdateBookingAndProperty()
    {
        // Arrange
        int loanId = 1;
        var loan = new LoanApplication
        {
            Id = loanId,
            BookingId = 100,
            BuyerId = 10,
            Status = "Pending"
        };
        
        var booking = new BookingEntity
        {
            Id = 100,
            PropertyId = 50,
            Status = "Pending"
        };
        
        var property = new PropertyEntity
        {
            Id = 50,
            Status = "Available",
            IsAvailable = true
        };

        _loanRepoMock.Setup(r => r.GetByIdAsync(loanId)).ReturnsAsync(loan);
        _bookingRepoMock.Setup(r => r.GetByIdAsync(loan.BookingId)).ReturnsAsync(booking);
        _propertyRepoMock.Setup(r => r.GetByIdAsync(booking.PropertyId)).ReturnsAsync(property);

        // Act
        var result = await _sut.UpdateLoanStatusAsync(loanId, "Approved");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Approved");
        _loanRepoMock.Verify(r => r.UpdateAsync(It.Is<LoanApplication>(l => l.Status == "Approved")), Times.Once);
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.Is<BookingEntity>(b => b.Status == "Booked")), Times.Once);
        _propertyRepoMock.Verify(r => r.UpdateAsync(It.Is<PropertyEntity>(p => p.Status == "Sold" && p.IsAvailable == false)), Times.Once);
    }
}
