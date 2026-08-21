using ApexWorld_Backend.Features.Users.DTOs;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Users.Services{
    public interface IUserService
    {
        Task<BuyerProfileDto?> GetBuyerProfileAsync(int userId);
        Task<bool> UpdateBuyerProfileAsync(int userId, UpdateBuyerProfileDto dto);
        Task<bool> DeleteBuyerAccountAsync(int userId);
        Task<AdminProfileDto?> GetAdminProfileAsync(int userId);
        Task<bool> UpdateAdminProfileAsync(int userId, UpdateAdminProfileDto dto);
        Task<bool> DeleteSubAdminAsync(int userId);
    }
}



