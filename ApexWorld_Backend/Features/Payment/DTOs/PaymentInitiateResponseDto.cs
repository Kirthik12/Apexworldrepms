namespace ApexWorld_Backend.Features.Payment.DTOs
{
    public class PaymentInitiateResponseDto
    {
        public int PaymentRecordId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentLinkUrl { get; set; }
        public string? TransactionId { get; set; }
    }
}
