using ApexWorld_Backend.Data;
using ApexWorld_Backend.Features.Booking.Services;
using ApexWorld.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;

namespace ApexWorld_Backend.Modules.Booking.Controllers
{
    [Tags("Admin - Site-Visit Booking")]
    [Route("api/v1/AdminBooking")]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBookingService _bookingService;

        public BookingController(ApplicationDbContext dbContext, IBookingService bookingService)
        {
            _dbContext = dbContext;
            _bookingService = bookingService;
        }

        [HttpGet]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SubAdmin)]
        public async Task<IActionResult> GetAllBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? onlyPurchased = null)
        {
            IQueryable<BookingEntity> query = _dbContext.Bookings.Include(b => b.Property);

            if (onlyPurchased == true)
            {
                query = query.Where(b => 
                    b.Status == "Paid" || 
                    b.Status == "Booked" || 
                    (b.PaymentReference != null && b.PaymentReference != "") ||
                    _dbContext.LoanApplications.Any(l => l.BookingId == b.Id && l.Status == "Approved")
                );
            }

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var b in bookings)
            {
                if (b.Status == "Booked" || _dbContext.LoanApplications.Any(l => l.BookingId == b.Id && l.Status == "Approved"))
                {
                    b.PaymentMethod = "Loan";
                }
                else
                {
                    var payment = await _dbContext.Payments
                        .Where(p => p.BookingId == b.Id && p.Status == "Success")
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefaultAsync();

                    b.PaymentMethod = payment != null 
                        ? (payment.PaymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase) ? "Card" :
                           payment.PaymentMethod.Equals("netbanking", StringComparison.OrdinalIgnoreCase) ? "Net Banking" :
                           payment.PaymentMethod.Equals("upi", StringComparison.OrdinalIgnoreCase) ? "UPI" : 
                           (payment.PaymentMethod.Equals("loan", StringComparison.OrdinalIgnoreCase) ? "Loan" : payment.PaymentMethod))
                        : (!string.IsNullOrEmpty(b.PaymentReference) ? "Online Payment" : "N/A");
                }
            }

            var response = new { TotalItems = totalCount, PageNumber = pageNumber, PageSize = pageSize, Items = bookings };

            return Ok(ApiResponse<object>.SuccessResponse(response));
        }

        [HttpGet("{id}")]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SubAdmin)]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound(ApiResponse<string>.ErrorResponse("Booking not found."));

            if (booking.Status == "Booked" || _dbContext.LoanApplications.Any(l => l.BookingId == booking.Id && l.Status == "Approved"))
            {
                booking.PaymentMethod = "Loan";
            }
            else
            {
                var payment = await _dbContext.Payments
                    .Where(p => p.BookingId == booking.Id && p.Status == "Success")
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                booking.PaymentMethod = payment != null 
                    ? (payment.PaymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase) ? "Card" :
                       payment.PaymentMethod.Equals("netbanking", StringComparison.OrdinalIgnoreCase) ? "Net Banking" :
                       payment.PaymentMethod.Equals("upi", StringComparison.OrdinalIgnoreCase) ? "UPI" : 
                       (payment.PaymentMethod.Equals("loan", StringComparison.OrdinalIgnoreCase) ? "Loan" : payment.PaymentMethod))
                    : (!string.IsNullOrEmpty(booking.PaymentReference) ? "Online Payment" : "N/A");
            }

            return Ok(ApiResponse<BookingEntity>.SuccessResponse(booking));
        }

        public class RejectRequest
        {
            public string Reason { get; set; } = string.Empty;
        }

        [HttpPost("{id}/approve")]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.SubAdmin + "," + Roles.Admin)]
        [ApexWorld_Backend.Filters.Idempotent]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            try
            {
                await _bookingService.ApproveBookingAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse("Booking approved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/reject")]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.SubAdmin + "," + Roles.Admin)]
        [ApexWorld_Backend.Filters.Idempotent]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] RejectRequest request)
        {
            try
            {
                await _bookingService.RejectBookingAsync(id, request.Reason);
                return Ok(ApiResponse<string>.SuccessResponse("Booking rejected successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }



        [HttpPost("{id}/reschedule/approve")]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.SubAdmin + "," + Roles.Admin)]
        public async Task<IActionResult> ApproveReschedule(int id)
        {
            try
            {
                await _bookingService.ApproveRescheduleAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse("Reschedule request approved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/reschedule/reject")]
        [Tags("Admin - Site-Visit Booking", "SubAdmin - Site-Visit Booking")]
        [Authorize(Roles = Roles.SubAdmin + "," + Roles.Admin)]
        public async Task<IActionResult> RejectReschedule(int id, [FromBody] RejectRequest request)
        {
            try
            {
                await _bookingService.RejectRescheduleAsync(id, request.Reason);
                return Ok(ApiResponse<string>.SuccessResponse("Reschedule request rejected."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
        [HttpPost("{id}/cancel/approve")]
        [Tags("Admin - Cancellation")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ApproveCancellation(int id)
        {
            try
            {
                await _bookingService.ApproveCancellationAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse("Cancellation request approved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/cancel/reject")]
        [Tags("Admin - Cancellation")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RejectCancellation(int id, [FromBody] RejectRequest request)
        {
            try
            {
                await _bookingService.RejectCancellationAsync(id, request.Reason);
                return Ok(ApiResponse<string>.SuccessResponse("Cancellation request rejected."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("cancellations")]
        [Tags("Admin - Cancellation")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllCancelledBookings()
        {
            var bookings = await _bookingService.GetAllCancelledBookingsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(bookings, "Cancelled bookings retrieved successfully."));
        }

        [HttpGet("cancellations/property/{propertyId}")]
        [Tags("Admin - Cancellation")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetCancelledBookingsByPropertyId(int propertyId)
        {
            var bookings = await _bookingService.GetCancelledBookingsByPropertyIdAsync(propertyId);
            return Ok(ApiResponse<object>.SuccessResponse(bookings, "Cancelled bookings retrieved successfully."));
        }

        [HttpGet("cancellations/user/{userId}")]
        [Tags("Admin - Cancellation")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetCancelledBookingsByUserId(string userId)
        {
            try
            {
                var bookings = await _bookingService.GetCancelledBookingsByUserIdAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(bookings, "Cancelled bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
