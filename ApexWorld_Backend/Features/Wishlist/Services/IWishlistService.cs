using ApexWorld_Backend.Features.Wishlist.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Wishlist.Services{
    public interface IWishlistService
    {

        Task<System.Collections.Generic.IEnumerable<ApexWorld_Backend.Features.Property.Models.Property>> GetWishlistPropertiesAsync(string buyerId);
        Task<bool> AddToWishlistAsync(string buyerId, int propertyId);
        Task<bool> RemoveFromWishlistAsync(string buyerId, int propertyId);
        Task<bool> RemoveRangeFromWishlistAsync(string buyerId, IEnumerable<int> propertyIds);
    }
}
