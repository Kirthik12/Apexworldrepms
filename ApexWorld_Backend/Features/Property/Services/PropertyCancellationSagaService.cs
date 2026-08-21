using System;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Booking.Models;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using ApexWorld_Backend.Features.Payment.Services;
using ApexWorld_Backend.Features.Property.Models;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ApexWorld_Backend.Features.Property.Services
{
    public interface IPropertyCancellationSagaService
    {
        Task InitiatePropertyCancellationAsync(int propertyId);
        Task ExecuteCancellationSagaAsync(int propertyId);
    }

    public class PropertyCancellationSagaService : IPropertyCancellationSagaService
    {
        private readonly IRepository<Models.Property> _propertyRepo;
        private readonly IRepository<BookingEntity> _bookingRepo;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IAuditService _auditService;
        private readonly ILogger<PropertyCancellationSagaService> _logger;

        public PropertyCancellationSagaService(
            IRepository<Models.Property> propertyRepo,
            IRepository<BookingEntity> bookingRepo,
            IPaymentService paymentService,
            IBackgroundJobClient backgroundJobs,
            IAuditService auditService,
            ILogger<PropertyCancellationSagaService> logger)
        {
            _propertyRepo = propertyRepo;
            _bookingRepo = bookingRepo;
            _paymentService = paymentService;
            _backgroundJobs = backgroundJobs;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task InitiatePropertyCancellationAsync(int propertyId)
        {
            // First, mark the property status as 'Archiving' to prevent new bookings
            var property = await _propertyRepo.GetByIdAsync(propertyId);
            if (property == null || property.IsDeleted) return;

            property.Status = "Archiving";
            await _propertyRepo.UpdateAsync(property);

            // Enqueue the saga background job
            _backgroundJobs.Enqueue(() => ExecuteCancellationSagaAsync(propertyId));
        }

        public async Task ExecuteCancellationSagaAsync(int propertyId)
        {
            try
            {
                _logger.LogInformation($"Starting cancellation saga for property {propertyId}");
                
                var activeBookings = (await _bookingRepo.GetAsync(b => b.PropertyId == propertyId && 
                    (b.Status == "Reserved" || b.Status == "Pending"))).ToList();

                foreach (var booking in activeBookings)
                {
                    _logger.LogInformation($"Cancelling booking {booking.Id} for property {propertyId}");
                    
                    // 1. Cancel booking
                    booking.Status = "Cancelled";
                    await _bookingRepo.UpdateAsync(booking);

                    // 2. Process Refund (Assuming payment exists)
                    var payments = await _paymentService.GetAdminPaymentsAsync(); // In real scenario, would fetch by bookingId
                    var bookingPayment = payments.FirstOrDefault(p => p.BookingId == booking.Id && p.Status == "Success");
                    
                    if (bookingPayment != null && !string.IsNullOrEmpty(bookingPayment.TransactionId))
                    {
                        // Delegate refund to PaymentService background job
                        _backgroundJobs.Enqueue<IPaymentService>(ps => ps.RefundPayment(bookingPayment.TransactionId, booking.Id, null));
                    }

                    await _auditService.LogAsync("BookingCancelled", "Booking", booking.Id.ToString(), $"Booking cancelled due to property archiving (PropertyId: {propertyId})", "System");
                }

                // Finally, mark the property as deleted
                var property = await _propertyRepo.GetByIdAsync(propertyId);
                if (property != null)
                {
                    property.IsDeleted = true;
                    property.Status = "Archived";
                    await _propertyRepo.UpdateAsync(property);
                    await _auditService.LogAsync("PropertyDeleted", "Property", property.Id.ToString(), $"Property successfully archived and bookings cancelled.", "System");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to complete cancellation saga for property {propertyId}");
                throw; // Let Hangfire DLQ catch it
            }
        }
    }
}
