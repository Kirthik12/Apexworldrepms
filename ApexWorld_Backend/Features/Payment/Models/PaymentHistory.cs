using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Payment.Models{
    public class PaymentHistory : BaseEntity
    {
        public int PaymentId { get; set; }
        public string StatusChange { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        public PaymentRecord? PaymentRecord { get; set; }
    }
}
