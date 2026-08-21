using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Wishlist.Models;
using ApexWorld_Backend.Features.Wishlist.DTOs;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld_Backend.Features.Wishlist.Services{
    public class WishlistService : IWishlistService
    {
        private readonly IRepository<Models.Wishlist> _wishlistRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Property.Models.Property> _propertyRepo;

        public WishlistService(
            IRepository<Models.Wishlist> wishlistRepo,
            IRepository<ApexWorld_Backend.Features.Property.Models.Property> propertyRepo)
        {
            _wishlistRepo = wishlistRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<System.Collections.Generic.IEnumerable<ApexWorld_Backend.Features.Property.Models.Property>> GetWishlistPropertiesAsync(string buyerIdStr)
        {
            if (!int.TryParse(buyerIdStr, out int buyerId))
            {
                throw new Exception("Invalid buyer ID.");
            }

            var wishlists = await _wishlistRepo.GetAsync(w => w.BuyerId == buyerId, "Property,Property.Images");
            return wishlists.Where(w => w.Property != null).Select(w => w.Property!).ToList();
        }



        public async Task<bool> AddToWishlistAsync(string buyerIdStr, int propertyId)
        {
            if (!int.TryParse(buyerIdStr, out int buyerId))
            {
                throw new Exception("Invalid buyer ID.");
            }

            var property = await _propertyRepo.GetByIdAsync(propertyId);
            if (property == null)
            {
                throw new Exception("Property not found.");
            }

            var existing = await _wishlistRepo.GetAsync(w => w.BuyerId == buyerId && w.PropertyId == propertyId);
            if (existing.Any())
            {
                return false; // Already in wishlist
            }

            var newWishlist = new Models.Wishlist
            {
                BuyerId = buyerId,
                PropertyId = propertyId
            };

            await _wishlistRepo.AddAsync(newWishlist);
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(string buyerIdStr, int propertyId)
        {
            if (!int.TryParse(buyerIdStr, out int buyerId))
            {
                throw new Exception("Invalid buyer ID.");
            }

            var wishlists = await _wishlistRepo.GetAsync(w => w.BuyerId == buyerId && w.PropertyId == propertyId);
            var itemToRemove = wishlists.FirstOrDefault();

            if (itemToRemove != null)
            {
                await _wishlistRepo.DeleteAsync(itemToRemove);
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveRangeFromWishlistAsync(string buyerIdStr, IEnumerable<int> propertyIds)
        {
            if (!int.TryParse(buyerIdStr, out int buyerId))
            {
                throw new Exception("Invalid buyer ID.");
            }

            var toRemove = await _wishlistRepo.GetAsync(w => w.BuyerId == buyerId && propertyIds.Contains(w.PropertyId));
            if (!toRemove.Any()) return false;

            foreach (var item in toRemove)
            {
                await _wishlistRepo.DeleteAsync(item);
            }
            return true;
        }
    }
}
