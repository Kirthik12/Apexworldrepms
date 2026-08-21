using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Loan.Models{
    public class Policy : BaseEntity
    {
        public string PolicyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Terms { get; set; } = string.Empty;
    }
}
