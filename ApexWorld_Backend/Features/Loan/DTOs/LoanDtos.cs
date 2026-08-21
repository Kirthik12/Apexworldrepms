namespace ApexWorld_Backend.Features.Loan.DTOs{
    public class LoanApplicationRequestDto
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public decimal LoanAmount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public int TenureYears { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
    }

    public class LoanStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty; // Approved, Rejected
    }
}
