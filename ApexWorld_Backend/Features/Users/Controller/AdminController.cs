using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Features.Users.Services;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Users.Controllers
{
    [Tags("Admin - SubAdmin Management")]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AdminController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("create-subadmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterAdminDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Username and password are required."));
                }

                if (string.IsNullOrWhiteSpace(request.Role))
                {
                    request.Role = Roles.SubAdmin;
                }

                var user = await _authService.RegisterAdminAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { user.Id, user.Username, Role = request.Role }, "SubAdmin created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("delete-subadmin/{id}")]
        public async Task<IActionResult> DeleteSubAdmin(string id)
        {
            try
            {
                if (!int.TryParse(id, out int subAdminId))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid ID format."));
                }

                var success = await _userService.DeleteSubAdminAsync(subAdminId);
                if (!success)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse("SubAdmin not found."));
                }

                return Ok(ApiResponse<string>.SuccessResponse("SubAdmin deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
