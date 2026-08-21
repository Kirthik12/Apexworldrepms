using ApexWorld.Core.Common;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Property.Models{
    public class Property : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        // Advanced Fields
        public string Address { get; set; } = string.Empty;
        public int CarpetArea { get; set; }
        public string Facing { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int AreaSize { get; set; }
        public string Furnishing { get; set; } = string.Empty;
        public int TotalFloors { get; set; }
        public decimal Maintenance { get; set; }
        public int CarParking { get; set; }
        
        // Category is now a Navigation Property
        public int CategoryId { get; set; }
        public PropertyCategory? Category { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        public string Status { get; set; } = "Pending"; // Pending, Available, Sold
        
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}
