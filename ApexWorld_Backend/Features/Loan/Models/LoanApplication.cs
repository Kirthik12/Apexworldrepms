using System;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Loan.Models{
    public class LoanApplication : BaseEntity
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public decimal LoanAmount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public int TenureYears { get; set; } = 20;
        public string? EmploymentType { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyEMI { get; set; }
        
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
        public int RejectionCount { get; set; } = 0;
        
        // Navigation Properties
        public ApexWorld_Backend.Features.Property.Models.Property? Property { get; set; }
        public ApexWorld_Backend.Features.Booking.Models.Booking? Booking { get; set; }
    }
}
