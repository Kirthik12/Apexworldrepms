using ApexWorld_Backend.Features.Review.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Review.Services
{
    public interface IReviewService
    {
        Task<int> AddPlatformReviewAsync(string buyerId, CreatePlatformReviewDto dto);
        Task<int> AddPropertyReviewAsync(string buyerId, CreatePropertyReviewDto dto);
        Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync(string? reviewType);
        Task<ReviewViewModel> GetReviewByIdAsync(int id);
        Task<IEnumerable<ReviewViewModel>> GetReviewsByBuyerIdAsync(string buyerId);
        Task DeleteReviewAsync(int id, string? buyerId = null);
        Task RespondToReviewAsync(int id, string adminResponse);
    }
}
