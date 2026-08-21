using System.Threading.Tasks;

namespace ApexWorld_Backend.Common.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, string entityId, string details, string? userIdOverride = null);
        Task CleanupOldLogsAsync(int retentionDays);
    }
}
