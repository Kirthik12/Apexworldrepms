using ApexWorld_Backend.Features.Users.Models;

namespace ApexWorld_Backend.Features.Roles.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
