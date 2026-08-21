using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Review.Rules
{
    public interface IReviewRule<T>
    {
        string? Validate(T request);
    }

    public class PlatformRatingRule : IReviewRule<DTOs.CreatePlatformReviewDto>
    {
        public string? Validate(DTOs.CreatePlatformReviewDto request)
        {
            if (request.Rating < 1 || request.Rating > 5) return "Rating must be between 1 and 5.";
            return null;
        }
    }

    public class PropertyRatingRule : IReviewRule<DTOs.CreatePropertyReviewDto>
    {
        public string? Validate(DTOs.CreatePropertyReviewDto request)
        {
            if (request.Rating < 1 || request.Rating > 5) return "Rating must be between 1 and 5.";
            return null;
        }
    }

    public class MaxPhotosRule : IReviewRule<DTOs.CreatePropertyReviewDto>
    {
        public string? Validate(DTOs.CreatePropertyReviewDto request)
        {
            if (request.Photos != null && request.Photos.Count > 10)
            {
                return "Maximum 10 photos can be uploaded.";
            }
            return null;
        }
    }
}

