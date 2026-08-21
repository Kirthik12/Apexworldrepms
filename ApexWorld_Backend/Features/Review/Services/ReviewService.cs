using ApexWorld_Backend.Features.Review.Exceptions;
using ApexWorld_Backend.Features.Review.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models; 
using ReviewEntity = ApexWorld_Backend.Features.Review.Models.Review;
using PropertyEntity = ApexWorld_Backend.Features.Property.Models.Property;
using UserEntity = ApexWorld_Backend.Features.Users.Models.User;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;

namespace ApexWorld_Backend.Features.Review.Services{
    public class ReviewService : ApexWorld_Backend.Features.Review.Services.IReviewService
    {
        private readonly IRepository<ReviewEntity> _reviewRepo;
        private readonly IRepository<BookingEntity> _bookingRepo;
        private readonly IRepository<PropertyEntity> _propertyRepo;
        private readonly IRepository<UserEntity> _userRepo;

        public ReviewService(
            IRepository<ReviewEntity> reviewRepo, 
            IRepository<BookingEntity> bookingRepo, 
            IRepository<PropertyEntity> propertyRepo,
            IRepository<UserEntity> userRepo)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<int> AddPlatformReviewAsync(string buyerId, CreatePlatformReviewDto dto)
        {
            int bId = int.Parse(buyerId);
            var existing = await _reviewRepo.GetAsync(r => r.BuyerId == bId && r.ReviewType == "Platform");
            if (existing.Any())
            {
                throw new ReviewNotAllowedException("You have already submitted a platform review.");
            }

            var review = new ReviewEntity
            {
                BuyerId = bId,
                ReviewType = "Platform",
                Rating = dto.Rating,
                Comment = dto.Comment,
                Tags = dto.Tags != null ? string.Join(", ", dto.Tags) : null
            };

            var added = await _reviewRepo.AddAsync(review);
            return added.Id;
        }

        public async Task<int> AddPropertyReviewAsync(string buyerId, CreatePropertyReviewDto dto)
        {
            // Validate Purchase or Site Visit via BookingId
            int bId = int.Parse(buyerId);
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);

            if (booking == null || booking.BuyerId != bId || booking.Status == "Cancelled" || booking.Status == "Failed")
            {
                throw new ReviewNotAllowedException("You can only review properties you have purchased or successfully visited.");
            }

            // Prevent duplicate review for the same property
            var existing = await _reviewRepo.GetAsync(r => r.BuyerId == bId && r.PropertyId == booking.PropertyId && r.ReviewType == "Property");
            if (existing.Any())
            {
                throw new ReviewNotAllowedException("You have already submitted a review for this property.");
            }

            var review = new ReviewEntity
            {
                BuyerId = bId,
                ReviewType = "Property",
                PropertyId = booking.PropertyId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Photos = dto.Photos != null ? string.Join(",", dto.Photos) : null
            };

            var added = await _reviewRepo.AddAsync(review);
            return added.Id;
        }

        public async Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync(string? reviewType)
        {
            var reviews = string.IsNullOrEmpty(reviewType) 
                ? await _reviewRepo.GetAsync(r => true, "Property") 
                : await _reviewRepo.GetAsync(r => r.ReviewType == reviewType, "Property");

            var buyerIds = reviews.Select(r => r.BuyerId).Distinct().ToList();
            var buyers = new Dictionary<int, string>();
            foreach (var bid in buyerIds)
            {
                var user = await _userRepo.GetByIdAsync(bid);
                if (user != null)
                {
                    buyers[bid] = user.FullName ?? user.Username;
                }
            }

            var viewModels = reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id, 
                BuyerId = r.BuyerId,
                ReviewType = r.ReviewType,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                BuyerName = buyers.ContainsKey(r.BuyerId) ? buyers[r.BuyerId] : "User " + r.BuyerId.ToString(),
                PropertyId = r.PropertyId,
                PropertyName = r.Property != null ? r.Property.Title : null,
                Tags = r.Tags?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
                Photos = r.Photos?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList(),
                Status = r.Status,
                AdminResponse = r.AdminResponse,
                ResponseDate = r.ResponseDate
            });

            return viewModels;
        }

        public async Task<ReviewViewModel> GetReviewByIdAsync(int id)
        {
            var reviews = await _reviewRepo.GetAsync(r => r.Id == id, "Property");
            var review = reviews.FirstOrDefault();
            if (review == null) return null!;

            var user = await _userRepo.GetByIdAsync(review.BuyerId);
            string buyerName = user?.FullName ?? user?.Username ?? ("User " + review.BuyerId.ToString());

            return new ReviewViewModel
            {
                Id = review.Id, 
                BuyerId = review.BuyerId,
                ReviewType = review.ReviewType,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                BuyerName = buyerName,
                PropertyId = review.PropertyId,
                PropertyName = review.Property?.Title,
                Status = review.Status,
                AdminResponse = review.AdminResponse,
                ResponseDate = review.ResponseDate
            };
        }

        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByBuyerIdAsync(string buyerId)
        {
            int bId = int.Parse(buyerId);
            var reviews = await _reviewRepo.GetAsync(r => r.BuyerId == bId, "Property");
            
            var user = await _userRepo.GetByIdAsync(bId);
            string buyerName = user?.FullName ?? user?.Username ?? ("User " + bId.ToString());

            return reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id, 
                BuyerId = r.BuyerId,
                ReviewType = r.ReviewType,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                BuyerName = buyerName,
                PropertyId = r.PropertyId,
                PropertyName = r.Property?.Title,
                Tags = r.Tags?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
                Photos = r.Photos?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList(),
                Status = r.Status,
                AdminResponse = r.AdminResponse,
                ResponseDate = r.ResponseDate
            });
        }

        public async Task DeleteReviewAsync(int id, string? buyerId = null)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            if (review != null)
            {
                if (buyerId != null && review.BuyerId != int.Parse(buyerId))
                {
                    throw new Exception("You are not authorized to delete this review.");
                }
                await _reviewRepo.DeleteAsync(review);
            }
        }

        public async Task RespondToReviewAsync(int id, string adminResponse)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                throw new KeyNotFoundException($"Review with ID {id} not found.");
            }
            review.Status = "Resolved";
            review.AdminResponse = adminResponse;
            review.ResponseDate = System.DateTime.UtcNow;
            await _reviewRepo.UpdateAsync(review);
        }
    }
}
