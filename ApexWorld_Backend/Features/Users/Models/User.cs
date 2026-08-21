using System;
using System.Collections.Generic;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Users.Models{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public ICollection<ApexWorld_Backend.Features.Roles.Models.UserRole> UserRoles { get; set; } = new List<ApexWorld_Backend.Features.Roles.Models.UserRole>();
    }
}
