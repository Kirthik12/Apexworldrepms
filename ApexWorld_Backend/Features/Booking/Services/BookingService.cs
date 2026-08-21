using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Booking.DTOs;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApexWorld_Backend.Features.Property.Exceptions;
using ApexWorld_Backend.Features.Booking.Exceptions;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Notifications.Services;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using PropertyEntity = ApexWorld_Backend.Features.Property.Models.Property;

namespace ApexWorld_Backend.Features.Booking.Services{
    public class BookingService : IBookingService
    {
        private readonly IRepository<BookingEntity> _bookingRepo;
        private readonly IRepository<PropertyEntity> _propertyRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IAuditService _auditService;
        private readonly ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService _webhookService;
        private readonly IBuyerNotificationService _buyerNotificationService;
        private readonly ApexWorld_Backend.Common.Services.IBulkheadService _bulkheadService;
        private readonly IMemoryCache _cache;
        private readonly IRepository<ApexWorld_Backend.Features.Payment.Models.PaymentRecord> _paymentRepo;
        private readonly IAdminNotificationService _adminNotificationService;

        public BookingService(
            IRepository<BookingEntity> bookingRepo, 
            IRepository<PropertyEntity> propertyRepo, 
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobs,
            IAuditService auditService,
            ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService webhookService,
            IBuyerNotificationService buyerNotificationService,
            ApexWorld_Backend.Common.Services.IBulkheadService bulkheadService,
            IMemoryCache cache,
            IRepository<ApexWorld_Backend.Features.Payment.Models.PaymentRecord> paymentRepo,
            IAdminNotificationService adminNotificationService)
        {
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _auditService = auditService;
            _webhookService = webhookService;
            _buyerNotificationService = buyerNotificationService;
            _bulkheadService = bulkheadService;
            _cache = cache;
            _paymentRepo = paymentRepo;
            _adminNotificationService = adminNotificationService;
        }

        private void InvalidatePropertySearchCache()
        {
            var options = new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove };
            _cache.Set("PropertySearch_CacheVersion", Guid.NewGuid().ToString(), options);
        }

        private async Task CreateBuyerBookingNotificationAsync(int buyerId, string title, string message, int bookingId)
        {
            await _buyerNotificationService.CreateBuyerNotificationAsync(new CreateBuyerNotificationDto
            {
                BuyerId = buyerId,
                Title = title,
                Message = message,
                Category = "Bookings",
                ActionText = "View Booking",
                ActionUrl = $"/api/v1/BuyerBooking/{bookingId}",
                RelatedEntityType = "Booking",
                RelatedEntityId = bookingId
            });
        }

        public async Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetBookingsByBuyerAsync(string buyerId)
        {
            if (!int.TryParse(buyerId, out int bId)) throw new ArgumentException("Invalid buyer ID.");
            return await _bookingRepo.GetAsync(b => b.BuyerId == bId, "Property,Property.Category,Property.Images");
        }

        public async Task<BookingEntity> GetBookingByBuyerAsync(int bookingId, string buyerId)
        {
            if (!int.TryParse(buyerId, out int bId)) throw new ArgumentException("Invalid buyer ID.");

            var bookings = await _bookingRepo.GetAsync(b => b.Id == bookingId && b.BuyerId == bId, "Property,Property.Category,Property.Images");
            var booking = bookings.FirstOrDefault();
            if (booking == null) throw new BookingNotFoundException(bookingId);

            return booking;
        }

        public async Task<System.Collections.Generic.IEnumerable<PropertyEntity>> GetPurchasedPropertiesByBuyerAsync(string buyerId)
        {
            if (!int.TryParse(buyerId, out int bId)) throw new ArgumentException("Invalid buyer ID.");

            var purchasedStatuses = new[] { "Paid", "Approved", "Booked" };
            var bookings = await _bookingRepo.GetAsync(
                b => b.BuyerId == bId && purchasedStatuses.Contains(b.Status),
                "Property,Property.Category,Property.Images");

            return bookings
                .Where(b => b.Property != null)
                .Select(b => b.Property!)
                .DistinctBy(p => p.Id)
                .ToList();
        }

        public async Task<PropertyEntity> GetPurchasedPropertyByBuyerAsync(int propertyId, string buyerId)
        {
            if (!int.TryParse(buyerId, out int bId)) throw new ArgumentException("Invalid buyer ID.");

            var purchasedStatuses = new[] { "Paid", "Approved", "Booked" };
            var bookings = await _bookingRepo.GetAsync(
                b => b.BuyerId == bId && b.PropertyId == propertyId && purchasedStatuses.Contains(b.Status),
                "Property,Property.Category,Property.Images");

            var property = bookings.Select(b => b.Property).FirstOrDefault(p => p != null);
            if (property == null) throw new PropertyNotFoundException(propertyId);

            return property;
        }

        public async Task<BookingEntity> InitiateBookingAsync(BookingRequestDto req)
        {
            return await _bulkheadService.ExecuteAsync("Booking", async () => 
            {
                var propertyId = req.PropertyId;
                var buyerId = req.BuyerId;
                var scheduledDate = req.ScheduledDate;

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var property = await _propertyRepo.GetByIdAsync(propertyId);
                    
                    if (property == null || !property.IsAvailable || property.IsDeleted || property.Status == "Booked")
                    {
                        throw new PropertyUnavailableException("This property is no longer available because it has already been booked.");
                    }

                    if (scheduledDate == null || scheduledDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        throw new Exception("Site visit must be scheduled for today or a future date.");
                    }

                    if (scheduledDate.Value.TimeOfDay < TimeSpan.FromHours(9) || scheduledDate.Value.TimeOfDay > TimeSpan.FromHours(16) || scheduledDate.Value.Minute != 0 || scheduledDate.Value.Second != 0)
                    {
                        throw new Exception("Site visits must be scheduled exactly on the hour, between 09:00 and 16:00 (for a 1-hour slot up to 17:00).");
                    }

                    var existingVisit = (await _bookingRepo.GetAsync(b => 
                        b.PropertyId == propertyId && 
                        b.BuyerId == buyerId && 
                        b.Status != "Cancelled" && 
                        b.Status != "Rejected")).FirstOrDefault();
                    
                    if (existingVisit != null)
                    {
                        throw new Exception("You already have an active booking for this property.");
                    }

                    var concurrentBookings = (await _bookingRepo.GetAsync(b => 
                        b.PropertyId == propertyId && 
                        b.ScheduledDate == scheduledDate.Value &&
                        b.Status != "Cancelled" && 
                        b.Status != "Rejected")).Count();

                    if (concurrentBookings >= 3)
                    {
                        throw new Exception("Maximum of 3 site visits allowed for this time slot.");
                    }

                    var booking = new BookingEntity
                    {
                        PropertyId = propertyId,
                        BuyerId = buyerId,
                        ScheduledDate = scheduledDate,
                        Status = "PendingAdminApproval", // Requires admin approval for site visit
                        FirstName = req.FirstName,
                        LastName = req.LastName,
                        Email = req.Email,
                        PhoneNumber = req.PhoneNumber,
                        PermanentAddress = req.PermanentAddress,
                    };
                    
                    await _bookingRepo.AddAsync(booking);

                    await _unitOfWork.CommitTransactionAsync();

                    await _auditService.LogAsync("Book", "Property", propertyId.ToString(), $"Buyer '{buyerId}' initiated booking", buyerId.ToString());
                    
                    await _webhookService.EnqueueEventAsync("Booking.Created", booking);
                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Site Visit Requested",
                        "Your site visit request has been submitted and is waiting for approval.",
                        booking.Id);

                    await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
                    {
                        Title = "New Site Visit Request",
                        Message = $"Buyer {booking.FirstName} {booking.LastName} requested a site visit for property {propertyId}.",
                        Category = "SiteVisit"
                    }, 0); // Assuming 0 for system sender or similar.

                    // Send Slack notification (background)
                    _backgroundJobs.Enqueue(() => Console.WriteLine($"Slack Alert: Booking {booking.Id} initiated for property {propertyId}."));
                    
                    InvalidatePropertySearchCache();

                    return booking;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Double booking detected. The property was modified by another user.");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }


        public async Task ApproveBookingAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);

            if (booking.Status != "Paid" && booking.Status != "PendingAdminApproval")
            {
                throw new Exception("Only paid purchases or pending site visits can be approved.");
            }

            if (booking.Status == "PendingAdminApproval")
            {
                booking.Status = "Approved";
                await _auditService.LogAsync("Approve", "Booking", bookingId.ToString(), $"Admin approved site visit.");
                await CreateBuyerBookingNotificationAsync(
                    booking.BuyerId,
                    "Booking Confirmed",
                    "Your site visit booking has been approved.",
                    booking.Id);
            }
            else 
            {
                booking.Status = "Approved";
                await _auditService.LogAsync("Approve", "Booking", bookingId.ToString(), $"Admin approved paid booking.");
            }
            
            await _bookingRepo.UpdateAsync(booking);
        }

        public async Task RejectBookingAsync(int bookingId, string reason)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                booking.Status = "Rejected";
                booking.RejectionReason = reason;
                await _bookingRepo.UpdateAsync(booking);

                var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                if (property != null)
                {
                    property.IsAvailable = true;
                    property.Status = "Available";
                    await _propertyRepo.UpdateAsync(property);
                }

                if (booking.Status == "Paid" && !string.IsNullOrEmpty(booking.PaymentReference))
                {
                    _backgroundJobs.Enqueue<ApexWorld_Backend.Features.Payment.Services.IPaymentService>(p => p.RefundPayment(booking.PaymentReference, booking.Id, null));
                }

                await _unitOfWork.CommitTransactionAsync();
                await _auditService.LogAsync("Reject", "Booking", bookingId.ToString(), $"Admin rejected booking. Reason: {reason}");
                await CreateBuyerBookingNotificationAsync(
                    booking.BuyerId,
                    "Booking Rejected",
                    string.IsNullOrWhiteSpace(reason) ? "Your site visit booking has been rejected." : $"Your site visit booking has been rejected. Reason: {reason}",
                    booking.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RequestRescheduleAsync(int bookingId, DateTime newDate, string buyerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null || booking.BuyerId != int.Parse(buyerId)) throw new Exception("Booking not found.");

            if (booking.ScheduledDate == null) throw new Exception("Existing booking has no scheduled date.");
            
            if (booking.ScheduledDate.Value.Date <= DateTime.UtcNow.Date)
            {
                throw new Exception("You can only reschedule a site visit before the scheduled visit date.");
            }

            if (newDate.Date <= DateTime.UtcNow.Date)
            {
                throw new Exception("The new date must be scheduled for a future date (next day onwards).");
            }

            if (newDate.TimeOfDay < TimeSpan.FromHours(9) || newDate.TimeOfDay > TimeSpan.FromHours(16) || newDate.Minute != 0 || newDate.Second != 0)
            {
                throw new Exception("Site visits must be scheduled exactly on the hour, between 09:00 and 16:00.");
            }

            var concurrentBookings = (await _bookingRepo.GetAsync(b => 
                b.PropertyId == booking.PropertyId && 
                b.ScheduledDate == newDate &&
                b.Status != "Cancelled" && 
                b.Status != "Rejected" &&
                b.Id != bookingId)).Count();

            if (concurrentBookings >= 3)
            {
                throw new Exception("Maximum of 3 site visits allowed for this time slot.");
            }

            booking.RequestedRescheduleDate = newDate;
            booking.Status = "RescheduleRequested";
            
            await _bookingRepo.UpdateAsync(booking);
            await _auditService.LogAsync("RescheduleRequest", "Booking", bookingId.ToString(), $"Buyer {buyerId} requested to reschedule site visit to {newDate}.", buyerId);
            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Reschedule Requested",
                $"Your request to reschedule your site visit to {newDate:dd MMM yyyy, hh:mm tt} is pending admin approval.",
                booking.Id);
            
            InvalidatePropertySearchCache();
        }

        public async Task ApproveRescheduleAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);
            
            if (booking.Status != "RescheduleRequested" || booking.RequestedRescheduleDate == null)
            {
                throw new Exception("This booking does not have a pending reschedule request.");
            }
            
            var newDate = booking.RequestedRescheduleDate.Value;
            booking.ScheduledDate = newDate;
            booking.RequestedRescheduleDate = null;
            booking.Status = "Approved";
            
            await _bookingRepo.UpdateAsync(booking);
            await _auditService.LogAsync("ApproveReschedule", "Booking", bookingId.ToString(), $"Admin approved site visit reschedule to {newDate}.", "System");
            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Reschedule Approved",
                $"Your site visit reschedule to {newDate:dd MMM yyyy, hh:mm tt} has been approved.",
                booking.Id);
                
            InvalidatePropertySearchCache();
        }

        public async Task RejectRescheduleAsync(int bookingId, string reason)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);
            
            if (booking.Status != "RescheduleRequested")
            {
                throw new Exception("This booking does not have a pending reschedule request.");
            }
            
            booking.RequestedRescheduleDate = null;
            booking.Status = "Approved"; // Revert to active
            
            await _bookingRepo.UpdateAsync(booking);
            await _auditService.LogAsync("RejectReschedule", "Booking", bookingId.ToString(), $"Admin rejected site visit reschedule. Reason: {reason}", "System");
            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Reschedule Rejected",
                string.IsNullOrWhiteSpace(reason) ? "Your site visit reschedule request was rejected. The original date remains." : $"Your site visit reschedule request was rejected. Reason: {reason}. The original date remains.",
                booking.Id);
        }

        public async Task RescheduleSiteVisitAsync(int bookingId, DateTime newDate)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);

            if (newDate.Date <= DateTime.UtcNow.Date)
            {
                throw new Exception("The new date must be scheduled for a future date (next day onwards).");
            }

            if (newDate.TimeOfDay < TimeSpan.FromHours(9) || newDate.TimeOfDay > TimeSpan.FromHours(16) || newDate.Minute != 0 || newDate.Second != 0)
            {
                throw new Exception("Site visits must be scheduled exactly on the hour, between 09:00 and 16:00.");
            }

            var concurrentBookings = (await _bookingRepo.GetAsync(b =>
                b.PropertyId == booking.PropertyId &&
                b.ScheduledDate == newDate &&
                b.Status != "Cancelled" &&
                b.Status != "Rejected" &&
                b.Id != bookingId)).Count();

            if (concurrentBookings >= 3)
            {
                throw new Exception("Maximum of 3 site visits allowed for this time slot.");
            }

            booking.ScheduledDate = newDate;

            await _bookingRepo.UpdateAsync(booking);
            await _auditService.LogAsync("Reschedule", "Booking", bookingId.ToString(), "SubAdmin/Admin rescheduled site visit.", "System");
            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Site Visit Rescheduled",
                $"Your site visit has been rescheduled to {newDate:dd MMM yyyy, hh:mm tt}.",
                booking.Id);

            InvalidatePropertySearchCache();
        }

        public async Task CancelStaleBookingsAsync()
        {
            // Find bookings that have been pending payment for more than 15 minutes
            var timeoutThreshold = DateTime.UtcNow.AddMinutes(-15);
            var staleBookings = await _bookingRepo.GetAsync(b => 
                (b.Status == "PendingPayment" || b.Status == "Pending" || b.Status == "Visited" || b.Status == "Interested") && 
                (b.UpdatedAt ?? b.CreatedAt) <= timeoutThreshold);

            foreach (var booking in staleBookings)
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    booking.Status = "Cancelled";
                    await _bookingRepo.UpdateAsync(booking);

                    var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                    if (property != null)
                    {
                        property.IsAvailable = true;
                        property.Status = "Available";
                        await _propertyRepo.UpdateAsync(property);
                    }

                    await _unitOfWork.CommitTransactionAsync();
                    await _auditService.LogAsync("Timeout", "Booking", booking.Id.ToString(), "System auto-cancelled stale reservation.", "System");
                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Booking Cancelled",
                        "Your booking was auto-cancelled because payment was not completed in time.",
                        booking.Id);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    // Log and continue to next booking
                    await _auditService.LogAsync("Error", "BookingTimeout", booking.Id.ToString(), $"Failed to cancel stale booking: {ex.Message}", "System");
                }
            }
        }

        public async Task CancelBookingDueToLoanRejectionAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasPaid = booking.Status == "Paid";
                var paymentRef = booking.PaymentReference;

                booking.Status = "Cancelled";
                await _bookingRepo.UpdateAsync(booking);

                var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                if (property != null)
                {
                    property.IsAvailable = true;
                    property.Status = "Available";
                    await _propertyRepo.UpdateAsync(property);
                }

                if (wasPaid && !string.IsNullOrEmpty(paymentRef))
                {
                    _backgroundJobs.Enqueue<ApexWorld_Backend.Features.Payment.Services.IPaymentService>(p => p.RefundPayment(paymentRef, booking.Id, null));
                }

                await _unitOfWork.CommitTransactionAsync();
                await _auditService.LogAsync("Cancel", "Booking", booking.Id.ToString(), "System auto-cancelled booking due to loan rejection.", "System");
                await CreateBuyerBookingNotificationAsync(
                    booking.BuyerId,
                    "Booking Cancelled",
                    "Your booking was cancelled because the loan application was rejected.",
                    booking.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CancelBookingAsync(int bookingId, string buyerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null || booking.BuyerId != int.Parse(buyerId)) throw new Exception("Booking not found.");

            if (booking.Status == "Cancelled" || booking.Status == "Rejected" || booking.Status == "Refunded")
            {
                booking.IsDeleted = true;
                await _bookingRepo.UpdateAsync(booking);
                return;
            }

            if (booking.Status == "CancellationRequested")
                throw new Exception("Cancellation is already pending approval.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasPaid = booking.Status == "Paid";

                if (wasPaid)
                {
                    // Check date-based rules
                    var payments = await _paymentRepo.GetAsync(p => p.BookingId == bookingId && p.Status == "Success");
                    var payment = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                    
                    if (payment != null)
                    {
                        var daysSincePayment = (DateTime.UtcNow - payment.CreatedAt).TotalDays;
                        if (daysSincePayment > 30)
                        {
                            throw new Exception("Cannot cancel booking more than 30 days after payment.");
                        }
                    }

                    booking.Status = "CancellationRequested";
                    await _bookingRepo.UpdateAsync(booking);
                    
                    // Notify Admin
                    await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
                    {
                        Title = "Booking Cancellation Requested",
                        Message = $"Buyer {buyerId} requested to cancel booking {booking.Id}.",
                        Category = "Cancellations",
                        TargetAudience = "Admins"
                    }, 0); // 0 or system admin id

                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Cancellation Requested",
                        "Your request to cancel the booking is pending admin approval.",
                        booking.Id);
                }
                else
                {
                    // Unpaid, just cancel directly
                    booking.Status = "Cancelled";
                    await _bookingRepo.UpdateAsync(booking);

                    var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                    if (property != null)
                    {
                        property.IsAvailable = true;
                        property.Status = "Available";
                        await _propertyRepo.UpdateAsync(property);
                    }

                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Booking Cancelled",
                        "Your booking has been cancelled successfully.",
                        booking.Id);
                }

                await _unitOfWork.CommitTransactionAsync();
                await _auditService.LogAsync("Cancel", "Booking", booking.Id.ToString(), $"Buyer {buyerId} requested cancellation.", buyerId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ApproveCancellationAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);
            if (booking.Status != "CancellationRequested") throw new Exception("Booking is not pending cancellation.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payments = await _paymentRepo.GetAsync(p => p.BookingId == bookingId && p.Status == "Success");
                var payment = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                
                decimal refundAmount = 0m;

                if (payment != null)
                {
                    var daysSincePayment = (DateTime.UtcNow - payment.CreatedAt).TotalDays;
                    // Token advance is dynamically read from payment.Amount
                    // <= 7 days = 10% fee (10% fee, 90% refund)
                    // 8-30 days = 100% fee (100% fee, 0 refund)
                    if (daysSincePayment <= 7)
                    {
                        decimal fee = payment.Amount * 0.10m;
                        refundAmount = payment.Amount - fee;
                    }
                    else if (daysSincePayment <= 30)
                    {
                        refundAmount = 0m;
                    }
                    else
                    {
                        throw new Exception("Cannot cancel booking more than 30 days after payment.");
                    }
                }

                booking.Status = "Cancelled";
                await _bookingRepo.UpdateAsync(booking);

                var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                if (property != null)
                {
                    property.IsAvailable = true;
                    property.Status = "Available";
                    await _propertyRepo.UpdateAsync(property);
                }

                if (payment != null)
                {
                    _backgroundJobs.Enqueue<ApexWorld_Backend.Features.Payment.Services.IPaymentService>(p => p.RefundPayment(payment.TransactionId ?? "", booking.Id, refundAmount));
                }

                await CreateBuyerBookingNotificationAsync(
                    booking.BuyerId,
                    "Cancellation Approved",
                    "Your booking cancellation has been approved. The amount will be credited to your account within 3 - 5 days.",
                    booking.Id);

                await _unitOfWork.CommitTransactionAsync();
                await _auditService.LogAsync("ApproveCancel", "Booking", booking.Id.ToString(), $"Admin approved cancellation for booking {bookingId}.", "Admin");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RejectCancellationAsync(int bookingId, string reason)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BookingNotFoundException(bookingId);
            if (booking.Status != "CancellationRequested") throw new Exception("Booking is not pending cancellation.");

            booking.Status = "Paid"; // Revert to Paid
            await _bookingRepo.UpdateAsync(booking);

            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Cancellation Rejected",
                $"Your request to cancel the booking was rejected. Reason: {reason}",
                booking.Id);

            await _auditService.LogAsync("RejectCancel", "Booking", booking.Id.ToString(), $"Admin rejected cancellation for booking {bookingId}.", "Admin");
        }

        public async Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetAllCancelledBookingsAsync()
        {
            return await _bookingRepo.GetAsync(
                b => b.Status == "Cancelled" || b.Status == "CancellationRequested", 
                "Property");
        }

        public async Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetCancelledBookingsByPropertyIdAsync(int propertyId)
        {
            return await _bookingRepo.GetAsync(
                b => b.PropertyId == propertyId && (b.Status == "Cancelled" || b.Status == "CancellationRequested"), 
                "Property");
        }

        public async Task<System.Collections.Generic.IEnumerable<BookingEntity>> GetCancelledBookingsByUserIdAsync(string userId)
        {
            if (!int.TryParse(userId, out int bId)) throw new ArgumentException("Invalid user ID.");
            return await _bookingRepo.GetAsync(
                b => b.BuyerId == bId && (b.Status == "Cancelled" || b.Status == "CancellationRequested"), 
                "Property");
        }

        public async Task MarkVisitedAsync(int bookingId, string buyerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null || booking.BuyerId != int.Parse(buyerId)) throw new Exception("Booking not found.");

            if (booking.Status != "Approved")
            {
                throw new Exception("Only approved site visits can be marked as visited.");
            }

            booking.IsVisited = true;
            booking.VisitedDate = DateTime.UtcNow;
            booking.Status = "Visited";

            await _bookingRepo.UpdateAsync(booking);
            await _auditService.LogAsync("MarkVisited", "Booking", bookingId.ToString(), $"Buyer '{buyerId}' marked site visit as completed.", buyerId);
            
            await CreateBuyerBookingNotificationAsync(
                booking.BuyerId,
                "Site Visit Completed",
                "You have successfully completed the site visit. Please indicate your purchase interest.",
                booking.Id);
        }

        public async Task RecordInterestOutcomeAsync(int bookingId, string outcome, string buyerId)
        {
            if (outcome != "Interested" && outcome != "NotInterested")
            {
                throw new ArgumentException("Invalid interest outcome.");
            }

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null || booking.BuyerId != int.Parse(buyerId)) throw new Exception("Booking not found.");

            if (booking.Status != "Visited" && !booking.IsVisited)
            {
                throw new Exception("Site visit must be marked completed before indicating interest.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                booking.InterestOutcome = outcome;

                if (outcome == "NotInterested")
                {
                    booking.Status = "Cancelled";
                    await _bookingRepo.UpdateAsync(booking);

                    var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                    if (property != null)
                    {
                        property.IsAvailable = true;
                        property.Status = "Available";
                        await _propertyRepo.UpdateAsync(property);
                    }

                    await _auditService.LogAsync("RecordInterest", "Booking", bookingId.ToString(), $"Buyer '{buyerId}' marked Not Interested. Booking cancelled and property released.", buyerId);
                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Interest Logged",
                        "You marked Not Interested. The property has been released and booking has been cancelled.",
                        booking.Id);
                }
                else
                {
                    booking.Status = "Interested";
                    await _bookingRepo.UpdateAsync(booking);

                    await _auditService.LogAsync("RecordInterest", "Booking", bookingId.ToString(), $"Buyer '{buyerId}' marked Interested.", buyerId);
                    await CreateBuyerBookingNotificationAsync(
                        booking.BuyerId,
                        "Interest Logged",
                        "You marked Interested. You can now proceed to the payment page to complete your purchase.",
                        booking.Id);
                }

                await _unitOfWork.CommitTransactionAsync();
                InvalidatePropertySearchCache();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
