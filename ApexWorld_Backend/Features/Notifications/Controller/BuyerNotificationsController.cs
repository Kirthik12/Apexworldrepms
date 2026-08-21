    using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Features.Notifications.Controller
{
    [ApiController]
    [Route("api/v1/buyer/notifications")]
    [Tags("Buyer - Notifications")]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Buyer)]
    public class BuyerNotificationsController : ControllerBase
    {
        private readonly IBuyerNotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public BuyerNotificationsController(IBuyerNotificationService notificationService, ICurrentUserService currentUserService)
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
                var buyerId = _currentUserService.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

                var notifications = await _notificationService.GetBuyerNotificationsAsync(buyerId, category, unreadOnly, pageNumber, pageSize);
                return Ok(ApiResponse<BuyerNotificationListDto>.SuccessResponse(notifications, "Notifications retrieved successfully."));
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
                var buyerId = _currentUserService.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

                var notification = await _notificationService.GetBuyerNotificationByIdAsync(buyerId, id);
                return Ok(ApiResponse<BuyerNotificationDto>.SuccessResponse(notification, "Notification retrieved successfully."));
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
                var buyerId = _currentUserService.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

                await _notificationService.MarkAsReadAsync(buyerId, id);
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
                var buyerId = _currentUserService.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(buyerId)) return Unauthorized();

                await _notificationService.MarkAllAsReadAsync(buyerId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "All notifications marked as read."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
