using System;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Features.Notifications.Controller
{
    [ApiController]
    [Route("api/v1/admin/notifications")]
    [Tags("Admin - Notifications")]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public AdminNotificationsController(IAdminNotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] string? category,
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!int.TryParse(_currentUserService.UserId, out int adminId)) return Unauthorized();

                var notifications = await _notificationService.GetAdminNotificationsAsync(adminId, category, unreadOnly, pageNumber, pageSize);
                return Ok(ApiResponse<AdminNotificationListDto>.SuccessResponse(notifications, "Admin notifications retrieved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                if (!int.TryParse(_currentUserService.UserId, out int adminId)) return Unauthorized();

                var notification = await _notificationService.GetAdminNotificationByIdAsync(adminId, id);
                return Ok(ApiResponse<AdminNotificationDto>.SuccessResponse(notification, "Notification retrieved."));
            }
            catch (ApexWorld_Backend.Common.Exceptions.NotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                if (!int.TryParse(_currentUserService.UserId, out int adminId)) return Unauthorized();

                await _notificationService.MarkAsReadAsync(adminId, id);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification marked as read."));
            }
            catch (ApexWorld_Backend.Common.Exceptions.NotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                if (!int.TryParse(_currentUserService.UserId, out int adminId)) return Unauthorized();

                await _notificationService.MarkAllAsReadAsync(adminId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "All notifications marked as read."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            try
            {
                if (!int.TryParse(_currentUserService.UserId, out int adminId)) return Unauthorized();

                await _notificationService.BroadcastNotificationAsync(dto, adminId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification successfully broadcasted."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
