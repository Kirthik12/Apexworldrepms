using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApexWorld_Backend.Features.Review.DTOs{
    public class CreatePlatformReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public List<string>? Tags { get; set; }
        
        [Required]
        public string Comment { get; set; } = string.Empty;
    }

    public class CreatePropertyReviewDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public List<string>? Photos { get; set; }
        
        [Required]
        public string Comment { get; set; } = string.Empty;
    }

    public class RespondToReviewDto
    {
        [Required]
        public string AdminResponse { get; set; } = string.Empty;
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public int? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public int Rating { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Photos { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? AdminResponse { get; set; }
        public System.DateTime? ResponseDate { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}

