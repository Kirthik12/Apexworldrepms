using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Payment.Models{
    public class Receipt : BaseEntity
    {
        public int PaymentId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string IssuedTo { get; set; } = string.Empty;
        
        public PaymentRecord? PaymentRecord { get; set; }
    }
}
