using System.ComponentModel.DataAnnotations.Schema;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Review.Models{
    public class Review : BaseEntity
    {
        public int BuyerId { get; set; }

        // "Platform" or "Property"
        public string ReviewType { get; set; } = string.Empty;
        [NotMapped]
        public string PropertyName { get; set; } = string.Empty;

        public int? PropertyId { get; set; }
        
        public int Rating { get; set; } // 1 to 5
        
        // Comma separated tags e.g., "UI / Design, Fast Performance"
        public string? Tags { get; set; }
        
        // Comma separated image URLs, up to 10
        public string? Photos { get; set; }
        
        public string Comment { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        public string? AdminResponse { get; set; }
        public DateTime? ResponseDate { get; set; }

        [ForeignKey("PropertyId")]
        public ApexWorld_Backend.Features.Property.Models.Property? Property { get; set; }
    }
}


