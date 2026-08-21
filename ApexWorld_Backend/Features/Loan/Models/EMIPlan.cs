using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Loan.Models{
    public class EMIPlan : BaseEntity
    {
        public int LoanApplicationId { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int Months { get; set; }
        public decimal InterestRate { get; set; }
        
        public LoanApplication? LoanApplication { get; set; }
    }
}
