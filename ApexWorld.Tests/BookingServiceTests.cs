using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Services;
using Hangfire;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ApexWorld.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private Mock<IRepository<Booking>> _bookingRepoMock;
        private Mock<IRepository<Property>> _propertyRepoMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IBackgroundJobClient> _backgroundJobClientMock;
        private Mock<IAuditService> _auditServiceMock;
        private Mock<ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService> _webhookServiceMock;
        private Mock<ApexWorld_Backend.Features.Notifications.Services.IBuyerNotificationService> _buyerNotificationServiceMock;
        private Mock<ApexWorld_Backend.Common.Services.IBulkheadService> _bulkheadServiceMock;
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private BookingService _bookingService;

        [SetUp]
        public void Setup()
        {
            _bookingRepoMock = new Mock<IRepository<Booking>>();
            _propertyRepoMock = new Mock<IRepository<Property>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _auditServiceMock = new Mock<IAuditService>();
            _webhookServiceMock = new Mock<ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService>();
            _buyerNotificationServiceMock = new Mock<ApexWorld_Backend.Features.Notifications.Services.IBuyerNotificationService>();
            _bulkheadServiceMock = new Mock<ApexWorld_Backend.Common.Services.IBulkheadService>();
            var paymentRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Payment.Models.PaymentRecord>>();
            var adminNotificationMock = new Mock<ApexWorld_Backend.Features.Notifications.Services.IAdminNotificationService>();
            _cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

            // Setup BulkheadService to execute the action immediately
            _bulkheadServiceMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task<Booking>>>()))
                .Returns<string, Func<Task<Booking>>>((name, action) => action());
            _bulkheadServiceMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task>>()))
                .Returns<string, Func<Task>>((name, action) => action());

            _bookingService = new BookingService(
                _bookingRepoMock.Object,
                _propertyRepoMock.Object,
                _unitOfWorkMock.Object,
                _backgroundJobClientMock.Object,
                _auditServiceMock.Object,
                _webhookServiceMock.Object,
                _buyerNotificationServiceMock.Object,
                _bulkheadServiceMock.Object,
                _cache,
                paymentRepoMock.Object,
                adminNotificationMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _cache?.Dispose();
        }

        [Test]
        public void InitiateBookingAsync_PropertyNotAvailable_ThrowsException()
        {
            // Arrange
            var property = new Property { Id = 1, IsAvailable = false };
            _propertyRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(property);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ApexWorld_Backend.Features.Property.Exceptions.PropertyUnavailableException>(async () => await _bookingService.InitiateBookingAsync(new ApexWorld_Backend.Features.Booking.DTOs.BookingRequestDto { PropertyId = 1, BuyerId = 1 }));
            Assert.That(ex.Message, Is.EqualTo("Property with ID 1 is unavailable. "));
        }

        [Test]
        public async Task CancelBookingDueToLoanRejectionAsync_ValidBooking_CancelsBookingAndFreesProperty()
        {
            // Arrange
            var booking = new Booking { Id = 1, PropertyId = 2, Status = "Pending" };
            var property = new Property { Id = 2, IsAvailable = false };
            
            _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(property);

            // Act
            await _bookingService.CancelBookingDueToLoanRejectionAsync(1);

            // Assert
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Assert.That(booking.Status, Is.EqualTo("Cancelled"));
            _bookingRepoMock.Verify(r => r.UpdateAsync(booking), Times.Once);

            Assert.That(property.IsAvailable, Is.True);
            _propertyRepoMock.Verify(r => r.UpdateAsync(property), Times.Once);

            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task CancelBookingAsync_ValidPaidBooking_TriggersRefundAndCancels()
        {
            // Arrange
            var booking = new Booking { Id = 1, PropertyId = 2, BuyerId = 1, Status = "Paid", PaymentReference = "tx123" };
            var property = new Property { Id = 2, IsAvailable = false };
            
            _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(property);

            // Act
            await _bookingService.CancelBookingAsync(1, "1");

            // Assert
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Assert.That(booking.Status, Is.EqualTo("Cancelled"));
            Assert.That(property.IsAvailable, Is.True);
            
            _backgroundJobClientMock.Verify(x => x.Create(
                It.Is<Hangfire.Common.Job>(j => j.Method.Name == "RefundPayment" && (string)j.Args[0] == "tx123"),
                It.IsAny<Hangfire.States.EnqueuedState>()), 
                Times.Once);

            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }
    }
}
