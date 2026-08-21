using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Users.Models
{
    public class RevokedToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}


