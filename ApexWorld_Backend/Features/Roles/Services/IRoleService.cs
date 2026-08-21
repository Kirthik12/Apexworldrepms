using ApexWorld_Backend.Features.Roles.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Roles.Services
{
    public interface IRoleService
    {
        Task<Role> GetRoleByIdAsync(int id);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> CreateRoleAsync(string roleName);
    }
}
