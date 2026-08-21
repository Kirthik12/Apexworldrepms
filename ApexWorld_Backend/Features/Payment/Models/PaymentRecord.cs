using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Payment.Models{
    public class PaymentRecord : BaseEntity
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int BuyerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Pending";
        
        // Navigation Properties
        public ApexWorld_Backend.Features.Booking.Models.Booking? Booking { get; set; }
        public ICollection<PaymentHistory> History { get; set; } = new List<PaymentHistory>();
        public Receipt? Receipt { get; set; }
    }
}
