using ApexWorld_Backend.Features.Users.Exceptions;
using ApexWorld_Backend.Features.Users.Validators;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Features.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Features.Users.Controller
{
    [Tags("Buyer - Profile Management")]
    [Route("api/v1/BuyerProfile")]
    [ApiController]
    public class BuyerProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UpdateBuyerProfileValidator _buyerValidator;
        private readonly ICurrentUserService _currentUserService;

        public BuyerProfileController(IUserService userService, UpdateBuyerProfileValidator buyerValidator, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _buyerValidator = buyerValidator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Buyer)]
        public async Task<IActionResult> GetBuyerProfile()
        {
            if (!int.TryParse(_currentUserService.UserId, out var userId)) return Unauthorized();

            var profile = await _userService.GetBuyerProfileAsync(userId);
            if (profile == null) return NotFound(ApiResponse<object>.ErrorResponse("Profile not found."));

            return Ok(ApiResponse<BuyerProfileDto>.SuccessResponse(profile, "Profile fetched successfully."));
        }

        [HttpPut]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Buyer)]
        public async Task<IActionResult> UpdateBuyerProfile([FromBody] UpdateBuyerProfileDto request)
        {
            if (!int.TryParse(_currentUserService.UserId, out var userId)) return Unauthorized();

            try
            {
                _buyerValidator.Validate(request);
                var success = await _userService.UpdateBuyerProfileAsync(userId, request);
                if (!success) return NotFound(ApiResponse<object>.ErrorResponse("Profile not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully."));
            }
            catch (AuthValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", ex.Errors)));
            }
        }

        [HttpDelete]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Buyer)]
        public async Task<IActionResult> DeleteBuyerProfile()
        {
            if (!int.TryParse(_currentUserService.UserId, out var userId)) return Unauthorized();

            var success = await _userService.DeleteBuyerAccountAsync(userId);
            if (!success) return NotFound(ApiResponse<object>.ErrorResponse("Profile not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Account deleted successfully."));
        }
    }
}
