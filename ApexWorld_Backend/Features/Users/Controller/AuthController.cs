using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Features.Users.Services;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Modules.Users.Controllers
{
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        [HttpPost("register-buyer")]
        [AllowAnonymous]
        [Tags("Public - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> RegisterBuyer([FromBody] RegisterBuyerDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Username and password are required."));
                }

                var user = await _authService.RegisterBuyerAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { user.Id, user.Username, Role = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? ApexWorld.Core.Common.Roles.Buyer }, "Buyer registered successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("register-admin")]
        [AllowAnonymous]
        [Tags("Public - Authentication", "Admin - Authentication")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Username and password are required."));
                }

                var user = await _authService.RegisterAdminAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { user.Id, user.Username, Role = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? request.Role }, "Admin registered successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Tags("Public - Authentication", "Admin - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Username and password are required."));
                }

                var tokens = await _authService.LoginAsync(request);
                return Ok(ApiResponse<TokenResponseDto>.SuccessResponse(tokens, "Login successful."));
            }
            catch (Exception ex)
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        [Tags("Public - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdToken))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Google ID token is required."));
                }

                var tokens = await _authService.GoogleLoginAsync(request.IdToken);
                return Ok(ApiResponse<TokenResponseDto>.SuccessResponse(tokens, "Google Login successful."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [Tags("Public - Authentication", "Admin - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Access token and refresh token are required."));
                }

                var tokens = await _authService.RefreshTokenAsync(request);
                return Ok(ApiResponse<TokenResponseDto>.SuccessResponse(tokens, "Token refreshed successfully."));
            }
            catch (Exception ex)
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        [Tags("Public - Authentication", "Admin - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (!string.IsNullOrEmpty(userId))
                {
                    await _authService.LogoutAsync(userId, request?.RefreshToken);
                }
                return Ok(ApiResponse<string>.SuccessResponse("Logged out successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [Tags("Public - Authentication")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse("If your email is registered, you will receive a reset token."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [Tags("Public - Authentication")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse("Password has been reset successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        [Tags("Admin - Authentication", "Buyer - Authentication")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResponse("User not authenticated."));
                }
                
                await _authService.ChangePasswordAsync(userId, request);
                return Ok(ApiResponse<string>.SuccessResponse("Password changed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
