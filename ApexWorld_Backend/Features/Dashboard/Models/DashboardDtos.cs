using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Dashboard.Models
{
    public class DashboardSummaryDto
    {
        public int ActiveListings { get; set; }
        public decimal TotalCompletedRevenue { get; set; }
        public int PendingLoans { get; set; }
        public int UnresolvedEnquiries { get; set; }
    }

    public class RevenueTrendDto
    {
        public string Period { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    public class PropertyCategoryDistributionDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class BookingStatusOverviewDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ActiveBookingLedgerDto
    {
        public int BookingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentPaymentLedgerDto
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
    }
}
