using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Booking.Models
{
    public class Booking : BaseEntity
    {
        public int PropertyId { get; set; }
        public int BuyerId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, PendingAdminApproval, Approved, Paid, Cancelled, Refunded, Rejected

        public DateTime? ScheduledDate { get; set; }
        public DateTime? RequestedRescheduleDate { get; set; }
        public string? PaymentReference { get; set; }

        // Buyer Information fields (for form capture)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PermanentAddress { get; set; }

        // Site visit audit fields
        public string? RejectionReason { get; set; }
        public bool IsVisited { get; set; }
        public DateTime? VisitedDate { get; set; }
        public string? InterestOutcome { get; set; }
        
        // Navigation Property
        public ApexWorld_Backend.Features.Property.Models.Property? Property { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? PaymentMethod { get; set; }
    }
}
