using System;
using System.Threading.Tasks;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;

namespace ApexWorld_Backend.Features.Booking.Services{
    public interface IBookingService
    {
        Task<BookingEntity> InitiateBookingAsync(ApexWorld_Backend.Features.Booking.DTOs.BookingRequestDto req);
        Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetBookingsByBuyerAsync(string buyerId);
        Task<BookingEntity> GetBookingByBuyerAsync(int bookingId, string buyerId);
        Task<System.Collections.Generic.IEnumerable<ApexWorld_Backend.Features.Property.Models.Property>> GetPurchasedPropertiesByBuyerAsync(string buyerId);
        Task<ApexWorld_Backend.Features.Property.Models.Property> GetPurchasedPropertyByBuyerAsync(int propertyId, string buyerId);
        Task ApproveBookingAsync(int bookingId);
        Task RejectBookingAsync(int bookingId, string reason);
        Task CancelBookingDueToLoanRejectionAsync(int bookingId);
        Task CancelBookingAsync(int bookingId, string buyerId);
        Task ApproveCancellationAsync(int bookingId);
        Task RejectCancellationAsync(int bookingId, string reason);
        Task RequestRescheduleAsync(int bookingId, DateTime newDate, string buyerId);
        Task ApproveRescheduleAsync(int bookingId);
        Task RejectRescheduleAsync(int bookingId, string reason);
        Task RescheduleSiteVisitAsync(int bookingId, DateTime newDate);
        Task CancelStaleBookingsAsync();
        Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetAllCancelledBookingsAsync();
        Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetCancelledBookingsByPropertyIdAsync(int propertyId);
        Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetCancelledBookingsByUserIdAsync(string userId);
        Task MarkVisitedAsync(int bookingId, string buyerId);
        Task RecordInterestOutcomeAsync(int bookingId, string outcome, string buyerId);
    }
}
