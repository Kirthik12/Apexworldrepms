using System;
using System.Linq;
using ApexWorld_Backend.Features.Reports.DTOs;

namespace ApexWorld_Backend.Features.Reports.Rules
{
    public interface IReportRule
    {
        string? Validate(ReportRequestDto request);
    }

    public class ValidReportTypeRule : IReportRule
    {
        public string? Validate(ReportRequestDto request)
        {
            var validTypes = new[] 
            { 
                ReportTypes.Sales, 
                ReportTypes.Enquiry, 
                ReportTypes.Payment, 
                ReportTypes.Loan, 
                ReportTypes.Booking,
                ReportTypes.Users,
                ReportTypes.Properties,
                ReportTypes.SiteVisit
            };
            if (!validTypes.Contains(request.ReportType))
            {
                return $"Invalid ReportType. Must be one of: {string.Join(", ", validTypes)}.";
            }
            return null;
        }
    }

    public class ValidReportFormatRule : IReportRule
    {
        public string? Validate(ReportRequestDto request)
        {
            var validFormats = new[] { ReportFormats.PDF, ReportFormats.Excel, ReportFormats.CSV };
            if (!validFormats.Contains(request.Format))
            {
                return $"Invalid Format. Must be one of: {string.Join(", ", validFormats)}.";
            }
            return null;
        }
    }

    public class ValidReportDateRule : IReportRule
    {
        public string? Validate(ReportRequestDto request)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                if (request.EndDate < request.StartDate)
                {
                    return "EndDate cannot be earlier than StartDate.";
                }
            }
            return null;
        }
    }

    public class ValidSortByRule : IReportRule
    {
        public string? Validate(ReportRequestDto request)
        {
            var validSorts = new[] { "Date", "Amount", "Property Name", "Buyer Name", "PropertyName", "BuyerName" };
            if (!string.IsNullOrEmpty(request.SortBy) && !validSorts.Contains(request.SortBy))
            {
                return $"Invalid SortBy. Must be one of: {string.Join(", ", validSorts)}.";
            }
            return null;
        }
    }
}
