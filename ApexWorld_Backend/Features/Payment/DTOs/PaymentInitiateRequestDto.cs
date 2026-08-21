namespace ApexWorld_Backend.Features.Payment.DTOs{
    public class PaymentInitiateRequestDto
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty; // Razorpay, NetBanking, DebitCreditCard, UPI
        public string? PaymentDetails { get; set; } // e.g. UPI ID, Account Number, etc.
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
