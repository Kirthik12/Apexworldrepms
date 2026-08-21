using ApexWorld_Backend.Features.Booking.DTOs;
using ApexWorld_Backend.Features.Booking.Services;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Filters;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Booking.Controllers
{
    [Tags("Buyer - Site-Visit Booking")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BuyerBookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;

        public BuyerBookingController(IBookingService bookingService, ICurrentUserService currentUserService)
        {
            _bookingService = bookingService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                var bookings = await _bookingService.GetBookingsByBuyerAsync(buyerId);
                return Ok(ApiResponse<System.Collections.Generic.IEnumerable<BookingEntity>>.SuccessResponse(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> GetMyBookingById(int id)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                var booking = await _bookingService.GetBookingByBuyerAsync(id, buyerId);
                return Ok(ApiResponse<BookingEntity>.SuccessResponse(booking, "Booking retrieved successfully."));
            }
            catch (ApexWorld_Backend.Features.Booking.Exceptions.BookingNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("purchased-properties")]
        [Authorize(Roles = Roles.Buyer)]
        [Tags("Buyer - My Bookings")]
        public async Task<IActionResult> GetMyPurchasedProperties()
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                var properties = await _bookingService.GetPurchasedPropertiesByBuyerAsync(buyerId);
                return Ok(ApiResponse<object>.SuccessResponse(properties, "Purchased properties retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("purchased-properties/{propertyId}")]
        [Authorize(Roles = Roles.Buyer)]
        [Tags("Buyer - My Bookings")]
        public async Task<IActionResult> GetMyPurchasedPropertyById(int propertyId)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                var property = await _bookingService.GetPurchasedPropertyByBuyerAsync(propertyId, buyerId);
                return Ok(ApiResponse<object>.SuccessResponse(property, "Purchased property retrieved successfully."));
            }
            catch (ApexWorld_Backend.Features.Property.Exceptions.PropertyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        public class RescheduleRequest
        {
            public DateTime NewDate { get; set; }
        }

        [HttpPatch("{id}/reschedule")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> RequestReschedule(int id, [FromBody] RescheduleRequest request)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                await _bookingService.RequestRescheduleAsync(id, request.NewDate, buyerId);
                return Ok(ApiResponse<string>.SuccessResponse("Reschedule request submitted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        // Edge Case 7, 23: Idempotent API Design
        [HttpPost("book")]
        [Idempotent]
        [Authorize(Roles = Roles.Buyer)]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Fixed")]
        public async Task<IActionResult> BookProperty([FromBody] BookingRequestDto request)
        {
            var validator = new ApexWorld_Backend.Features.Booking.Validators.BookingRequestValidator();
            var (isValid, errors) = validator.Validate(request);
            if (!isValid) return BadRequest(ApiResponse<string>.ErrorResponse(string.Join(", ", errors)));

            try
            {
                if (int.TryParse(_currentUserService.UserId, out int parsedBuyerId))
                {
                    request.BuyerId = parsedBuyerId;
                }
                var booking = await _bookingService.InitiateBookingAsync(request);
                var message = "Booking initiated successfully. You can complete the payment in the payment section.";
                return Ok(ApiResponse<BookingEntity>.SuccessResponse(booking, message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Tags("Buyer - Cancellation")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                await _bookingService.CancelBookingAsync(id, buyerId);
                return Ok(ApiResponse<string>.SuccessResponse("Booking cancelled successfully. If you have already paid, your refund will be processed according to our policy."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("{id}/mark-visited")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> MarkVisited(int id)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                await _bookingService.MarkVisitedAsync(id, buyerId);
                return Ok(ApiResponse<string>.SuccessResponse("Booking marked as visited successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        public class InterestRequest
        {
            public string Interest { get; set; } = string.Empty;
        }

        [HttpPatch("{id}/interest")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> RecordInterest(int id, [FromBody] InterestRequest request)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                await _bookingService.RecordInterestOutcomeAsync(id, request.Interest, buyerId);
                return Ok(ApiResponse<string>.SuccessResponse("Interest outcome recorded successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
