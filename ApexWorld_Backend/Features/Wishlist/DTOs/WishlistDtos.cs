using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Wishlist.DTOs{
    public class WishlistItemDto
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? AvailabilityMessage { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class WishlistSummaryDto
    {
        public int TotalProperties { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AveragePrice { get; set; }
        public List<WishlistItemDto> Items { get; set; } = new();
    }
}

