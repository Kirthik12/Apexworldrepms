using System;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Enquiry.Models{
    public class Enquiry : BaseEntity
    {
        public string BuyerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        public string Status { get; set; } = "New"; // "New", "InProgress", "Resolved"
        public string? AdminResponse { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}

