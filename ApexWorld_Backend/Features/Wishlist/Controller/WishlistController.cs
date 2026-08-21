using ApexWorld_Backend.Features.Wishlist.Exceptions;
using ApexWorld_Backend.Features.Wishlist.Services;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Wishlist.DTOs;
using ApexWorld_Backend.Features.Wishlist.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApexWorld_Backend.Common.Exceptions;

namespace ApexWorld_Backend.Modules.Wishlist.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("Buyer - Wishlists")]
    [Authorize(Roles = Roles.Buyer)]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICurrentUserService _currentUserService;
        private readonly WishlistRequestValidator _validator;

        public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService, WishlistRequestValidator validator)
        {
            _wishlistService = wishlistService;
            _currentUserService = currentUserService;
            _validator = validator;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> GetWishlistProperties()
        {
            try
            {
                var buyerId = _currentUserService.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

                var properties = await _wishlistService.GetWishlistPropertiesAsync(buyerId);
                return Ok(ApiResponse<object>.SuccessResponse(new { Items = properties }, "Wishlist properties fetched successfully."));
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("wishlist_err.txt", ex.ToString());
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.ToString()));
            }
        }



        [HttpPost("{propertyId}")]
        public async Task<IActionResult> AddToWishlist(int propertyId)
        {
            try
            {
                _validator.ValidatePropertyId(propertyId);
            }
            catch (WishlistValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", ex.Errors)));
            }

            var buyerId = _currentUserService.UserId ?? string.Empty;
            if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

            var success = await _wishlistService.AddToWishlistAsync(buyerId, propertyId);
            if (!success) return NotFound(ApiResponse<object>.ErrorResponse("Property not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Property added to wishlist successfully."));
        }

        [HttpDelete("{propertyId}")]
        public async Task<IActionResult> RemoveFromWishlist(int propertyId)
        {
            var buyerId = _currentUserService.UserId ?? string.Empty;
            if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

            var success = await _wishlistService.RemoveFromWishlistAsync(buyerId, propertyId);
            if (!success) return NotFound(ApiResponse<object>.ErrorResponse("Property not found in wishlist."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Property removed from wishlist successfully."));
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkRemoveFromWishlist([FromBody] System.Collections.Generic.List<int> propertyIds)
        {
            if (propertyIds == null || propertyIds.Count == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Property IDs list cannot be empty."));
            }

            var buyerId = _currentUserService.UserId ?? string.Empty;
            if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

            var success = await _wishlistService.RemoveRangeFromWishlistAsync(buyerId, propertyIds);
            if (!success) return NotFound(ApiResponse<object>.ErrorResponse("No properties found in wishlist."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Properties removed from wishlist successfully."));
        }
    }
}
