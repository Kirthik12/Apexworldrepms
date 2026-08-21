using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Reports.Models;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Reports.DTOs;
using System.Linq;
using System.Text.Json;

namespace ApexWorld_Backend.Features.Reports.Services{
    public class ReportResult 
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ReportId { get; set; }
        public string DataPayload { get; set; } = string.Empty;
    }
    
    public class ReportService : IReportService
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Booking.Models.Booking> _bookingRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Payment.Models.PaymentRecord> _paymentRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Loan.Models.LoanApplication> _loanRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Enquiry.Models.Enquiry> _enquiryRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Property.Models.Property> _propertyRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Users.Models.User> _userRepo;

        public ReportService(
            IRepository<Report> reportRepo,
            IRepository<ApexWorld_Backend.Features.Booking.Models.Booking> bookingRepo,
            IRepository<ApexWorld_Backend.Features.Payment.Models.PaymentRecord> paymentRepo,
            IRepository<ApexWorld_Backend.Features.Loan.Models.LoanApplication> loanRepo,
            IRepository<ApexWorld_Backend.Features.Enquiry.Models.Enquiry> enquiryRepo,
            IRepository<ApexWorld_Backend.Features.Property.Models.Property> propertyRepo,
            IRepository<ApexWorld_Backend.Features.Users.Models.User> userRepo)
        {
            _reportRepo = reportRepo;
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _loanRepo = loanRepo;
            _enquiryRepo = enquiryRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<ReportResult> GenerateReportAsync(ReportRequestDto requestDto, string generatedBy = "Admin")
        {
            object reportData = new object();
            
            // Build filter queries based on request parameters
            switch (requestDto.ReportType)
            {
                case ReportTypes.Booking:
                    var bookings = await _bookingRepo.GetAllAsync();
                    var bookingQuery = bookings.AsQueryable();
                    if (requestDto.StartDate.HasValue)
                        bookingQuery = bookingQuery.Where(b => b.CreatedAt >= requestDto.StartDate.Value);
                    if (requestDto.EndDate.HasValue)
                        bookingQuery = bookingQuery.Where(b => b.CreatedAt <= requestDto.EndDate.Value);
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                        bookingQuery = bookingQuery.Where(b => b.Status == requestDto.ReportStatus);
                    if (!string.IsNullOrEmpty(requestDto.BookingId) && int.TryParse(requestDto.BookingId, out int bId))
                        bookingQuery = bookingQuery.Where(b => b.Id == bId);
                    
                    reportData = bookingQuery.Select(b => new {
                        b.Id,
                        b.PropertyId,
                        b.BuyerId,
                        b.Status,
                        b.ScheduledDate,
                        CustomerName = $"{b.FirstName} {b.LastName}",
                        b.Email,
                        b.PhoneNumber,
                        b.CreatedAt
                    }).ToList();
                    break;

                case ReportTypes.Payment:
                case ReportTypes.Sales:
                    var payments = await _paymentRepo.GetAllAsync();
                    var paymentQuery = payments.AsQueryable();
                    if (requestDto.StartDate.HasValue)
                        paymentQuery = paymentQuery.Where(p => p.CreatedAt >= requestDto.StartDate.Value);
                    if (requestDto.EndDate.HasValue)
                        paymentQuery = paymentQuery.Where(p => p.CreatedAt <= requestDto.EndDate.Value);
                    if (!string.IsNullOrEmpty(requestDto.PaymentMethod))
                        paymentQuery = paymentQuery.Where(p => p.PaymentMethod == requestDto.PaymentMethod);
                    if (!string.IsNullOrEmpty(requestDto.TransactionId))
                        paymentQuery = paymentQuery.Where(p => p.TransactionId == requestDto.TransactionId);
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                        paymentQuery = paymentQuery.Where(p => p.Status == requestDto.ReportStatus);

                    reportData = paymentQuery.Select(p => new {
                        p.Id,
                        p.BookingId,
                        p.PropertyId,
                        p.BuyerId,
                        p.Amount,
                        p.PaymentMethod,
                        p.TransactionId,
                        p.Status,
                        p.CreatedAt
                    }).ToList();
                    break;

                case ReportTypes.Loan:
                    var loans = await _loanRepo.GetAllAsync();
                    var loanQuery = loans.AsQueryable();
                    if (requestDto.StartDate.HasValue)
                        loanQuery = loanQuery.Where(l => l.CreatedAt >= requestDto.StartDate.Value);
                    if (requestDto.EndDate.HasValue)
                        loanQuery = loanQuery.Where(l => l.CreatedAt <= requestDto.EndDate.Value);
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                        loanQuery = loanQuery.Where(l => l.Status == requestDto.ReportStatus);
                    if (!string.IsNullOrEmpty(requestDto.BookingId) && int.TryParse(requestDto.BookingId, out int lbId))
                        loanQuery = loanQuery.Where(l => l.BookingId == lbId);

                    reportData = loanQuery.Select(l => new {
                        l.Id,
                        l.BuyerId,
                        l.BuyerName,
                        l.BookingId,
                        l.PropertyId,
                        l.LoanAmount,
                        l.BankName,
                        l.TenureYears,
                        l.EmploymentType,
                        l.MonthlyIncome,
                        l.MonthlyEMI,
                        l.Status,
                        l.CreatedAt
                    }).ToList();
                    break;

                case ReportTypes.Enquiry:
                case ReportTypes.SiteVisit:
                    var enquiries = await _enquiryRepo.GetAllAsync();
                    var enquiryQuery = enquiries.AsQueryable();
                    if (requestDto.StartDate.HasValue)
                        enquiryQuery = enquiryQuery.Where(e => e.CreatedAt >= requestDto.StartDate.Value);
                    if (requestDto.EndDate.HasValue)
                        enquiryQuery = enquiryQuery.Where(e => e.CreatedAt <= requestDto.EndDate.Value);
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                        enquiryQuery = enquiryQuery.Where(e => e.Status == requestDto.ReportStatus);
                    if (requestDto.ReportType == ReportTypes.SiteVisit)
                        enquiryQuery = enquiryQuery.Where(e => e.Message.Contains("site visit") || e.Message.Contains("appointment"));

                    reportData = enquiryQuery.Select(e => new {
                        e.Id,
                        e.BuyerName,
                        e.Phone,
                        e.Email,
                        e.Message,
                        e.Status,
                        e.AdminResponse,
                        e.ResponseDate,
                        e.CreatedAt
                    }).ToList();
                    break;

                case ReportTypes.Properties:
                    var props = await _propertyRepo.GetAllAsync();
                    var propQuery = props.AsQueryable();
                    if (!string.IsNullOrEmpty(requestDto.PropertyType))
                        propQuery = propQuery.Where(p => p.ProjectName.Contains(requestDto.PropertyType));
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                        propQuery = propQuery.Where(p => p.Status == requestDto.ReportStatus);

                    reportData = propQuery.Select(p => new {
                        p.Id,
                        p.Title,
                        p.ProjectName,
                        p.Price,
                        p.Address,
                        p.CarpetArea,
                        p.Bedrooms,
                        p.Bathrooms,
                        p.Status,
                        p.IsAvailable,
                        p.CreatedAt
                    }).ToList();
                    break;

                case ReportTypes.Users:
                    var users = await _userRepo.GetAllAsync();
                    var userQuery = users.AsQueryable();
                    if (!string.IsNullOrEmpty(requestDto.ReportStatus))
                    {
                        bool active = requestDto.ReportStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
                        userQuery = userQuery.Where(u => u.IsActive == active);
                    }

                    reportData = userQuery.Select(u => new {
                        u.Id,
                        u.Username,
                        u.FullName,
                        u.Email,
                        u.PhoneNumber,
                        u.City,
                        u.IsActive,
                        u.CreatedAt
                    }).ToList();
                    break;

                default:
                    reportData = new { Message = "Empty Report Payload" };
                    break;
            }

            var payload = JsonSerializer.Serialize(reportData);

            // Check if scheduled for future
            string status = "Completed";
            if (requestDto.StartDate.HasValue && requestDto.StartDate.Value > DateTime.UtcNow)
            {
                status = "Scheduled";
            }

            var report = new Report
            {
                ReportName = requestDto.ReportName,
                ReportType = requestDto.ReportType,
                PropertyScope = string.IsNullOrEmpty(requestDto.PropertyType) ? "All" : requestDto.PropertyType,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = status,
                GeneratedBy = generatedBy,
                Format = requestDto.Format,
                DataPayload = payload,
                GeneratedAt = DateTime.UtcNow
            };

            await _reportRepo.AddAsync(report);

            return new ReportResult
            {
                Success = true,
                Message = $"{requestDto.ReportType} report generated successfully.",
                ReportId = report.Id,
                DataPayload = payload
            };
        }

        public async Task<IEnumerable<ReportResponseDto>> GetReportsAsync(ReportFilterDto filter)
        {
            var reports = await _reportRepo.GetAllAsync();
            var query = reports.AsQueryable();

            if (!string.IsNullOrEmpty(filter.ReportType) && filter.ReportType != "All")
            {
                query = query.Where(r => r.ReportType.Equals(filter.ReportType, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
            {
                query = query.Where(r => r.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(r => 
                    r.ReportName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.ReportType.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.GeneratedBy.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)
                );
            }
            if (!string.IsNullOrEmpty(filter.DateRange) && filter.DateRange != "All")
            {
                if (filter.DateRange == "Future")
                {
                    query = query.Where(r => r.Status == "Scheduled");
                }
                else if (int.TryParse(filter.DateRange, out int days))
                {
                    var limit = DateTime.UtcNow.AddDays(-days);
                    query = query.Where(r => r.GeneratedAt >= limit);
                }
            }

            return query.OrderByDescending(r => r.GeneratedAt).Select(r => new ReportResponseDto
            {
                Id = r.Id,
                ReportName = r.ReportName,
                ReportType = r.ReportType,
                PropertyScope = r.PropertyScope,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                GeneratedOn = r.GeneratedAt,
                GeneratedBy = r.GeneratedBy,
                Status = r.Status,
                Format = r.Format,
                FileUrl = r.FileUrl
            }).ToList();
        }

        public async Task<ReportDashboardStatsDto> GetDashboardStatsAsync()
        {
            var reports = await _reportRepo.GetAllAsync();
            var list = reports.ToList();

            var stats = new ReportDashboardStatsDto
            {
                TotalReports = list.Count,
                BookingReports = list.Count(r => r.ReportType == ReportTypes.Booking),
                PaymentReports = list.Count(r => r.ReportType == ReportTypes.Payment),
                LoanReports = list.Count(r => r.ReportType == ReportTypes.Loan),
                SiteVisitReports = list.Count(r => r.ReportType == ReportTypes.SiteVisit || r.ReportType == "Site-Visit"),
                SalesReports = list.Count(r => r.ReportType == ReportTypes.Sales),
                EnquiryReports = list.Count(r => r.ReportType == ReportTypes.Enquiry),
                UsersReports = list.Count(r => r.ReportType == ReportTypes.Users),
                PropertiesReports = list.Count(r => r.ReportType == ReportTypes.Properties),
                CompletedReports = list.Count(r => r.Status == "Completed"),
                ScheduledReports = list.Count(r => r.Status == "Scheduled"),
                FailedReports = list.Count(r => r.Status == "Failed")
            };

            // Calculate trends (Current 30 days vs Previous 30 days)
            var now = DateTime.UtcNow;
            var currentLimit = now.AddDays(-30);
            var prevLimit = now.AddDays(-60);

            var currReports = list.Where(r => r.GeneratedAt >= currentLimit).ToList();
            var prevReports = list.Where(r => r.GeneratedAt >= prevLimit && r.GeneratedAt < currentLimit).ToList();

            stats.Trend = new ReportTrendDto
            {
                TotalReportsTrendPercent = GetPercentageTrend(currReports.Count, prevReports.Count),
                BookingTrendPercent = GetPercentageTrend(
                    currReports.Count(r => r.ReportType == ReportTypes.Booking),
                    prevReports.Count(r => r.ReportType == ReportTypes.Booking)
                ),
                PaymentTrendPercent = GetPercentageTrend(
                    currReports.Count(r => r.ReportType == ReportTypes.Payment || r.ReportType == ReportTypes.Sales),
                    prevReports.Count(r => r.ReportType == ReportTypes.Payment || r.ReportType == ReportTypes.Sales)
                ),
                LoanTrendPercent = GetPercentageTrend(
                    currReports.Count(r => r.ReportType == ReportTypes.Loan),
                    prevReports.Count(r => r.ReportType == ReportTypes.Loan)
                ),
                SiteVisitTrendPercent = GetPercentageTrend(
                    currReports.Count(r => r.ReportType == ReportTypes.SiteVisit || r.ReportType == "Site-Visit"),
                    prevReports.Count(r => r.ReportType == ReportTypes.SiteVisit || r.ReportType == "Site-Visit")
                )
            };

            return stats;
        }

        public async Task<ReportChartDataDto> GetChartDataAsync(string period)
        {
            var reports = await _reportRepo.GetAllAsync();
            var list = reports.ToList();

            var data = new ReportChartDataDto();
            
            // Build Type Split donut data
            data.TypeSplit = list
                .GroupBy(r => r.ReportType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build Status Split donut data
            data.StatusSplit = list
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build Generation Trend labels and series
            int daysToQuery = period.ToLower() switch
            {
                "daily" => 7,
                "weekly" => 30,
                "monthly" => 90,
                _ => 30
            };

            var cutoff = DateTime.UtcNow.AddDays(-daysToQuery);
            var filtered = list.Where(r => r.GeneratedAt >= cutoff).OrderBy(r => r.GeneratedAt).ToList();

            if (period.ToLower() == "daily")
            {
                for (int i = daysToQuery - 1; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddDays(-i).Date;
                    data.TrendLabels.Add(date.ToString("MMM dd"));
                    data.GeneratedSeries.Add(filtered.Count(r => r.GeneratedAt.Date == date));
                    data.DownloadedSeries.Add(filtered.Count(r => r.GeneratedAt.Date == date && r.Status == "Completed") / 2); // Simulated downloads for ratio visual
                }
            }
            else // Weekly/Monthly interval summary
            {
                var grouped = filtered
                    .GroupBy(r => r.GeneratedAt.ToString("yyyy-MM-dd"))
                    .Select(g => new { Label = g.Key, Count = g.Count() })
                    .ToList();

                data.TrendLabels = grouped.Select(g => g.Label).ToList();
                data.GeneratedSeries = grouped.Select(g => g.Count).ToList();
                data.DownloadedSeries = grouped.Select(g => Math.Max(1, g.Count - 1)).ToList();
            }

            return data;
        }

        public async Task<ReportResponseDto?> GetReportByIdAsync(int id)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null) return null;

            return new ReportResponseDto
            {
                Id = report.Id,
                ReportName = report.ReportName,
                ReportType = report.ReportType,
                PropertyScope = report.PropertyScope,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
                GeneratedOn = report.GeneratedAt,
                GeneratedBy = report.GeneratedBy,
                Status = report.Status,
                Format = report.Format,
                FileUrl = report.FileUrl
            };
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null) return false;

            await _reportRepo.DeleteAsync(report);
            return true;
        }

        public async Task<ReportResponseDto?> UpdateReportAsync(int id, ReportRequestDto dto)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null) return null;

            report.ReportName = dto.ReportName;
            report.ReportType = dto.ReportType;
            report.Format = dto.Format;
            report.PropertyScope = dto.PropertyType;
            report.StartDate = dto.StartDate;
            report.EndDate = dto.EndDate;
            report.Status = dto.ReportStatus ?? report.Status;

            await _reportRepo.UpdateAsync(report);
            return await GetReportByIdAsync(id);
        }

        private double GetPercentageTrend(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100.0 : 0.0;
            return Math.Round(((double)(current - previous) / previous) * 100.0, 1);
        }
    }
}
