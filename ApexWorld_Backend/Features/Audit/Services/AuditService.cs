using ApexWorld_Backend.Features.Audit.Exceptions;
using ApexWorld_Backend.Features.Audit.Models;
using ApexWorld_Backend.Features.Audit.DTOs;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Audit.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Audit.Services{
    public class AuditService : IAuditService, ApexWorld_Backend.Features.Audit.Services.IAuditQueryService
    {
        private readonly IRepository<AuditLog> _auditRepo;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(IRepository<AuditLog> auditRepo, ICurrentUserService currentUserService)
        {
            _auditRepo = auditRepo;
            _currentUserService = currentUserService;
        }

        public async Task LogAsync(string action, string entityType, string entityId, string details, string? userIdOverride = null)
        {
            var validator = new AuditRequestValidator();
            var (isValid, errors) = validator.Validate(action, entityType);
            
            if (!isValid)
            {
                throw new AuditLogFailedException(string.Join(", ", errors));
            }

            var auditLog = new AuditLog
            {
                UserId = userIdOverride ?? _currentUserService.UserId ?? "System",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details
            };

            await _auditRepo.AddAsync(auditLog);
        }

        public async Task<(IEnumerable<AuditDto> Items, int TotalCount)> GetAuditLogsAsync(int pageNumber, int pageSize)
        {
            var allLogs = await _auditRepo.GetAllAsync();
            var totalCount = allLogs.Count;

            var items = allLogs
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Action = x.Action,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    Details = x.Details,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return (items, totalCount);
        }

        public async Task CleanupOldLogsAsync(int retentionDays)
        {
            var cutoffDate = System.DateTime.UtcNow.AddDays(-retentionDays);
            var oldLogs = await _auditRepo.GetAsync(a => a.CreatedAt < cutoffDate);
            
            if (oldLogs != null && oldLogs.Any())
            {
                // Delete them one by one as IRepository does not have DeleteRangeAsync
                foreach (var log in oldLogs)
                {
                    await _auditRepo.DeleteAsync(log);
                }
            }
        }
    }
}






