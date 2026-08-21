using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApexWorld_Backend.Features.Reports.DTOs{
    public class ReportRequestDto
    {
        [Required]
        public string ReportName { get; set; } = string.Empty;
        
        [Required]
        public string ReportType { get; set; } = string.Empty; // Sales, Enquiry, Payment, Loan, Booking, Users, Properties, Site-Visit

        [Required]
        public string Format { get; set; } = string.Empty; // PDF, Excel, CSV

        public string PropertyType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ReportStatus { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Custom report configurations
        public string SortBy { get; set; } = "Date";
        public string SortOrder { get; set; } = "Descending";
        public bool IncludeSummary { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeTables { get; set; } = true;
        public bool IncludeStatistics { get; set; }
        public bool IncludeTransactionHistory { get; set; }
        public bool IncludePaymentBreakdown { get; set; }
        public bool IncludeBookingDetails { get; set; }
        public string? BuyerName { get; set; }
        public string? PropertyName { get; set; }
        public string? BookingId { get; set; }
        public string? TransactionId { get; set; }
    }

    public class ReportResponseDto
    {
        public int Id { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string PropertyScope { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
    }

    public class ReportFilterDto
    {
        public string? ReportType { get; set; }
        public string? Status { get; set; }
        public string? DateRange { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class ReportDashboardStatsDto
    {
        public int TotalReports { get; set; }
        public int BookingReports { get; set; }
        public int PaymentReports { get; set; }
        public int LoanReports { get; set; }
        public int SiteVisitReports { get; set; }
        public int SalesReports { get; set; }
        public int EnquiryReports { get; set; }
        public int UsersReports { get; set; }
        public int PropertiesReports { get; set; }
        public int CompletedReports { get; set; }
        public int ScheduledReports { get; set; }
        public int FailedReports { get; set; }
        public ReportTrendDto? Trend { get; set; }
    }

    public class ReportTrendDto
    {
        public double TotalReportsTrendPercent { get; set; }
        public double BookingTrendPercent { get; set; }
        public double PaymentTrendPercent { get; set; }
        public double LoanTrendPercent { get; set; }
        public double SiteVisitTrendPercent { get; set; }
    }

    public class ReportChartDataDto
    {
        public List<string> TrendLabels { get; set; } = new();
        public List<int> GeneratedSeries { get; set; } = new();
        public List<int> DownloadedSeries { get; set; } = new();
        public Dictionary<string, int> TypeSplit { get; set; } = new();
        public Dictionary<string, int> StatusSplit { get; set; } = new();
    }

    public static class ReportTypes
    {
        public const string Sales = "Sales";
        public const string Enquiry = "Enquiry";
        public const string Payment = "Payment";
        public const string Loan = "Loan";
        public const string Booking = "Booking";
        public const string Users = "Users";
        public const string Properties = "Properties";
        public const string SiteVisit = "Site-Visit";
    }

    public static class ReportFormats
    {
        public const string PDF = "PDF";
        public const string Excel = "Excel";
        public const string CSV = "CSV";
    }

    public static class ReportDurations
    {
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Custom = "Custom";
    }
}
