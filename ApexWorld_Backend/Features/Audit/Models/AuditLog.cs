using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Audit.Models{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}

