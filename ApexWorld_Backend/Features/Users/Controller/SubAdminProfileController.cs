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
    [Tags("SubAdmin - Profile Management")]
    [Route("api/v1/SubAdminProfile")]
    [ApiController]
    public class SubAdminProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UpdateAdminProfileValidator _adminValidator;
        private readonly ICurrentUserService _currentUserService;

        public SubAdminProfileController(IUserService userService, UpdateAdminProfileValidator adminValidator, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _adminValidator = adminValidator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.SubAdmin)]
        public async Task<IActionResult> GetAdminProfile()
        {
            if (!int.TryParse(_currentUserService.UserId, out var userId)) return Unauthorized();

            var profile = await _userService.GetAdminProfileAsync(userId);
            if (profile == null) return NotFound(ApiResponse<object>.ErrorResponse("Profile not found."));

            return Ok(ApiResponse<AdminProfileDto>.SuccessResponse(profile, "Profile fetched successfully."));
        }

        [HttpPut]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.SubAdmin)]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateAdminProfileDto request)
        {
            if (!int.TryParse(_currentUserService.UserId, out var userId)) return Unauthorized();

            try
            {
                _adminValidator.Validate(request);
                var success = await _userService.UpdateAdminProfileAsync(userId, request);
                if (!success) return NotFound(ApiResponse<object>.ErrorResponse("Profile not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully."));
            }
            catch (AuthValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", ex.Errors)));
            }
        }
    }
}
