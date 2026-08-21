using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Booking.Services;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Booking.DTOs;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Webhooks.Services;
using ApexWorld_Backend.Common.Services;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using ApexWorld_Backend.Features.Payment.Models;

namespace ApexWorld.UnitTests.Features.Booking;

public class BookingServiceTests
{
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>> _bookingRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobsMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IWebhookDispatchService> _webhookServiceMock;
    private readonly Mock<IBuyerNotificationService> _buyerNotificationServiceMock;
    private readonly Mock<IBulkheadService> _bulkheadServiceMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IRepository<PaymentRecord>> _paymentRepoMock;
    private readonly Mock<IAdminNotificationService> _adminNotificationServiceMock;
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _bookingRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>>();
        _propertyRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _backgroundJobsMock = new Mock<IBackgroundJobClient>();
        _auditServiceMock = new Mock<IAuditService>();
        _webhookServiceMock = new Mock<IWebhookDispatchService>();
        _buyerNotificationServiceMock = new Mock<IBuyerNotificationService>();
        _bulkheadServiceMock = new Mock<IBulkheadService>();
        _cacheMock = new Mock<IMemoryCache>();
        _paymentRepoMock = new Mock<IRepository<PaymentRecord>>();
        _adminNotificationServiceMock = new Mock<IAdminNotificationService>();

        // Setup Bulkhead to just execute the func directly
        _bulkheadServiceMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task<ApexWorld_Backend.Features.Booking.Models.Booking>>>()))
            .Returns<string, Func<Task<ApexWorld_Backend.Features.Booking.Models.Booking>>>((key, func) => func());

        // Setup MemoryCache mock
        var cacheEntryMock = new Mock<ICacheEntry>();
        _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        _sut = new BookingService(
            _bookingRepoMock.Object,
            _propertyRepoMock.Object,
            _unitOfWorkMock.Object,
            _backgroundJobsMock.Object,
            _auditServiceMock.Object,
            _webhookServiceMock.Object,
            _buyerNotificationServiceMock.Object,
            _bulkheadServiceMock.Object,
            _cacheMock.Object,
            _paymentRepoMock.Object,
            _adminNotificationServiceMock.Object
        );
    }

    [Fact]
    public async Task GetBookingsByBuyerAsync_WhenValidBuyerId_ShouldReturnBookings()
    {
        // Arrange
        var buyerId = "1";
        var expectedBookings = new List<ApexWorld_Backend.Features.Booking.Models.Booking>
        {
            new ApexWorld_Backend.Features.Booking.Models.Booking { Id = 10, BuyerId = 1, Status = "Pending" }
        };

        _bookingRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Booking.Models.Booking, bool>>>(), "Property"))
            .ReturnsAsync(expectedBookings);

        // Act
        var result = await _sut.GetBookingsByBuyerAsync(buyerId);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.First().Id.Should().Be(10);
    }

    [Fact]
    public async Task InitiateBookingAsync_WhenPropertyUnavailable_ShouldThrowException()
    {
        // Arrange
        var req = new BookingRequestDto
        {
            PropertyId = 100,
            BuyerId = 1,
            ScheduledDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10)
        };

        // Return a property that is NOT available
        var property = new ApexWorld_Backend.Features.Property.Models.Property { Id = 100, IsAvailable = false };
        _propertyRepoMock.Setup(r => r.GetByIdAsync(req.PropertyId)).ReturnsAsync(property);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _sut.InitiateBookingAsync(req);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*unavailable*");
    }

    [Fact]
    public async Task ApproveBookingAsync_WhenPendingAdminApproval_ShouldUpdateToPendingPayment()
    {
        // Arrange
        var bookingId = 1;
        var booking = new ApexWorld_Backend.Features.Booking.Models.Booking
        {
            Id = bookingId,
            BuyerId = 5,
            Status = "PendingAdminApproval"
        };

        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ApexWorld_Backend.Features.Booking.Models.Booking>())).Returns(Task.CompletedTask);

        // Act
        await _sut.ApproveBookingAsync(bookingId);

        // Assert
        booking.Status.Should().Be("PendingPayment");
        _bookingRepoMock.Verify(r => r.UpdateAsync(booking), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync("Approve", "Booking", bookingId.ToString(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
