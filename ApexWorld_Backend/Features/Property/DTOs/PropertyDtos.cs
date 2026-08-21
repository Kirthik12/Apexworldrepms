namespace ApexWorld_Backend.Features.Property.DTOs{
    public class PropertyCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty; // Apartment, Villa, Plot, Commercial Buildings

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
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
        public System.Collections.Generic.List<string> ImageUrls { get; set; } = new();
    }

    public class PropertyUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Furnishing { get; set; } = string.Empty;
        public int TotalFloors { get; set; }
        public decimal Maintenance { get; set; }
    }

    public class PropertyStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty; // Pending, Approved, Sold
        public bool IsAvailable { get; set; }
    }
}
