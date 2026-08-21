using ApexWorld_Backend.Features.Review.Exceptions;
using ApexWorld_Backend.Features.Review.DTOs;
using ApexWorld_Backend.Features.Review.Services;
using System;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Review.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Review.Controllers
{
    [Tags("Buyer - Reviews")]
    [Route("api/v1/BuyerReview")]
    [ApiController]
    public class BuyerReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly PlatformReviewValidator _platformValidator;
        private readonly PropertyReviewValidator _propertyValidator;
        private readonly ICurrentUserService _currentUserService;

        public BuyerReviewController(IReviewService reviewService, PlatformReviewValidator platformValidator, PropertyReviewValidator propertyValidator, ICurrentUserService currentUserService)
        {
            _reviewService = reviewService;
            _platformValidator = platformValidator;
            _propertyValidator = propertyValidator;
            _currentUserService = currentUserService;
        }

        [HttpPost("platform")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> AddPlatformReview([FromBody] CreatePlatformReviewDto request)
        {
            try
            {
                _platformValidator.Validate(request);
                var buyerId = _currentUserService.UserId ?? "Unknown";
                var id = await _reviewService.AddPlatformReviewAsync(buyerId, request);
                return Ok(ApiResponse<int>.SuccessResponse(id, "Platform review submitted successfully."));
            }
            catch (ReviewValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", ex.Errors)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("property")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> AddPropertyReview([FromBody] CreatePropertyReviewDto request)
        {
            try
            {
                _propertyValidator.Validate(request);
                var buyerId = _currentUserService.UserId ?? "Unknown";
                var id = await _reviewService.AddPropertyReviewAsync(buyerId, request);
                return Ok(ApiResponse<int>.SuccessResponse(id, "Property review submitted successfully."));
            }
            catch (ReviewValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", ex.Errors)));
            }
            catch (ReviewNotAllowedException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var buyerId = _currentUserService.UserId ?? "Unknown";
                var reviews = await _reviewService.GetReviewsByBuyerIdAsync(buyerId);
                return Ok(ApiResponse<object>.SuccessResponse(reviews, "Reviews fetched successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Buyer)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var buyerId = _currentUserService.UserId ?? "Unknown";
                await _reviewService.DeleteReviewAsync(id, buyerId);
                return Ok(ApiResponse<object>.SuccessResponse(null!, "Review deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
