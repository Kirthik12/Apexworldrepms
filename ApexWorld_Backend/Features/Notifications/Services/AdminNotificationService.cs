using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Notifications.Models;
using ApexWorld_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ApexWorld_Backend.Features.Notifications.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminNotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<AdminNotificationListDto> GetAdminNotificationsAsync(int adminId, string? category, bool unreadOnly, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.AdminNotifications.Where(n => n.AdminId == adminId && !n.IsDeleted);

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(n => n.Category == category);
            }

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var totalItems = await query.CountAsync();
            var unreadCount = await _context.AdminNotifications.CountAsync(n => n.AdminId == adminId && !n.IsDeleted && !n.IsRead);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => ToDto(n))
                .ToListAsync();

            return new AdminNotificationListDto
            {
                TotalItems = totalItems,
                UnreadCount = unreadCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<AdminNotificationDto> GetAdminNotificationByIdAsync(int adminId, int notificationId)
        {
            var notification = await _context.AdminNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.AdminId == adminId && !n.IsDeleted);

            if (notification == null) throw new NotFoundException("Notification not found.");
            return ToDto(notification);
        }

        public async Task MarkAsReadAsync(int adminId, int notificationId)
        {
            var notification = await _context.AdminNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.AdminId == adminId && !n.IsDeleted);

            if (notification == null) throw new NotFoundException("Notification not found.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int adminId)
        {
            var unreadNotifications = await _context.AdminNotifications
                .Where(n => n.AdminId == adminId && !n.IsDeleted && !n.IsRead)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
            }

            if (unreadNotifications.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task BroadcastNotificationAsync(BroadcastNotificationDto dto, int senderAdminId)
        {
            var targetAdmins = new List<int>();
            var targetBuyers = new List<int>();
            var allUserIds = new List<int>();

            if (dto.TargetAudience == "AllUsers")
            {
                targetAdmins = await _context.Admins.Select(a => a.Id).ToListAsync();
                targetBuyers = await _context.Buyers.Select(b => b.Id).ToListAsync();
            }
            else if (dto.TargetAudience == "Buyers")
            {
                targetBuyers = await _context.Buyers.Select(b => b.Id).ToListAsync();
            }
            else if (dto.TargetAudience == "Admins")
            {
                targetAdmins = await _context.Admins.Select(a => a.Id).ToListAsync();
            }
            else if (dto.TargetAudience == "SpecificRole" && !string.IsNullOrEmpty(dto.TargetRole))
            {
                var userIdsWithRole = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.Role.RoleName == dto.TargetRole)
                    .Select(ur => ur.UserId)
                    .ToListAsync();
                
                // We don't easily know which are admins/buyers just from UserId unless we query the TPT mapping
                var admins = await _context.Admins.Where(a => userIdsWithRole.Contains(a.Id)).Select(a => a.Id).ToListAsync();
                var buyers = await _context.Buyers.Where(b => userIdsWithRole.Contains(b.Id)).Select(b => b.Id).ToListAsync();
                targetAdmins.AddRange(admins);
                targetBuyers.AddRange(buyers);
            }
            else if (dto.TargetAudience == "SpecificUser" && dto.TargetUserId.HasValue)
            {
                var targetUserId = dto.TargetUserId.Value;
                var isAdmin = await _context.Admins.AnyAsync(a => a.Id == targetUserId);
                if (isAdmin) targetAdmins.Add(targetUserId);
                else
                {
                    var isBuyer = await _context.Buyers.AnyAsync(b => b.Id == targetUserId);
                    if (isBuyer) targetBuyers.Add(targetUserId);
                }
            }

            var now = DateTime.UtcNow;

            var adminNotifications = targetAdmins.Select(id => new AdminNotification
            {
                AdminId = id,
                Title = dto.Title,
                Message = dto.Message,
                Category = dto.Category,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            var buyerNotifications = targetBuyers.Select(id => new BuyerNotification
            {
                BuyerId = id,
                Title = dto.Title,
                Message = dto.Message,
                Category = dto.Category,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            if (adminNotifications.Any()) _context.AdminNotifications.AddRange(adminNotifications);
            if (buyerNotifications.Any()) _context.BuyerNotifications.AddRange(buyerNotifications);

            await _context.SaveChangesAsync();

            allUserIds.AddRange(targetAdmins);
            allUserIds.AddRange(targetBuyers);

            var userGroups = allUserIds.Select(id => $"User_{id}").ToList();
            if (userGroups.Any())
            {
                await _hubContext.Clients.Groups(userGroups).SendAsync("ReceiveNotification", new
                {
                    dto.Title,
                    dto.Message,
                    dto.Category,
                    CreatedAt = now
                });
            }
        }

        private static AdminNotificationDto ToDto(AdminNotification n)
        {
            return new AdminNotificationDto
            {
                Id = n.Id,
                AdminId = n.AdminId,
                Title = n.Title,
                Message = n.Message,
                Category = n.Category,
                ActionText = n.ActionText,
                ActionUrl = n.ActionUrl,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            };
        }
    }
}
