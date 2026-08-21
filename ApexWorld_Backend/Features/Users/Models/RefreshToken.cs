using System;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Users.Models{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        
        // Navigation Property
        public User? User { get; set; }
    }
}
