using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Features.Dashboard.Models;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Dashboard.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardSummaryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardSummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary()
        {
            var activeListings = await _context.Properties
                .CountAsync(p => p.IsAvailable && p.Status == "Available");

            var completedRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" || p.Status == "Success")
                .SumAsync(p => p.Amount);

            var pendingLoans = await _context.LoanApplications
                .CountAsync(l => l.Status == "Pending");

            var unresolvedEnquiries = await _context.Enquiries
                .CountAsync(e => e.Status == "New" || e.Status == "InProgress");

            return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(new DashboardSummaryDto
            {
                ActiveListings = activeListings,
                TotalCompletedRevenue = completedRevenue,
                PendingLoans = pendingLoans,
                UnresolvedEnquiries = unresolvedEnquiries
            }));
        }

        [HttpGet("revenue-trend")]
        public async Task<ActionResult<ApiResponse<RevenueTrendDto>>> GetRevenueTrend([FromQuery] string period = "monthly")
        {
            var query = _context.Payments
                .Where(p => p.Status == "Completed" || p.Status == "Success");

            var trendDto = new RevenueTrendDto { Period = period };
            var now = DateTime.UtcNow;

            if (period.ToLower() == "daily")
            {
                var startDate = now.AddDays(-6).Date;
                var payments = await query
                    .Where(p => p.CreatedAt >= startDate)
                    .ToListAsync();

                for (int i = 0; i < 7; i++)
                {
                    var day = startDate.AddDays(i);
                    trendDto.Labels.Add(day.ToString("MMM dd"));
                    trendDto.Data.Add(payments.Where(p => p.CreatedAt.Date == day).Sum(p => p.Amount));
                }
            }
            else if (period.ToLower() == "weekly")
            {
                var startDate = now.AddDays(-28).Date;
                var payments = await query
                    .Where(p => p.CreatedAt >= startDate)
                    .ToListAsync();

                for (int i = 0; i < 4; i++)
                {
                    var weekStart = startDate.AddDays(i * 7);
                    var weekEnd = weekStart.AddDays(7);
                    trendDto.Labels.Add($"Week {i + 1}");
                    trendDto.Data.Add(payments.Where(p => p.CreatedAt >= weekStart && p.CreatedAt < weekEnd).Sum(p => p.Amount));
                }
            }
            else
            {
                var startDate = now.AddMonths(-11).Date;
                startDate = new DateTime(startDate.Year, startDate.Month, 1);
                var payments = await query
                    .Where(p => p.CreatedAt >= startDate)
                    .ToListAsync();

                for (int i = 0; i < 12; i++)
                {
                    var month = startDate.AddMonths(i);
                    trendDto.Labels.Add(month.ToString("MMM"));
                    trendDto.Data.Add(payments.Where(p => p.CreatedAt.Year == month.Year && p.CreatedAt.Month == month.Month).Sum(p => p.Amount));
                }
            }

            return Ok(ApiResponse<RevenueTrendDto>.SuccessResponse(trendDto));
        }

        [HttpGet("property-category-distribution")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PropertyCategoryDistributionDto>>>> GetPropertyCategoryDistribution()
        {
            var distribution = await _context.Properties
                .Include(p => p.Category)
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new PropertyCategoryDistributionDto
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<PropertyCategoryDistributionDto>>.SuccessResponse(distribution));
        }

        [HttpGet("booking-status-overview")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingStatusOverviewDto>>>> GetBookingStatusOverview([FromQuery] string period = "monthly")
        {
            var now = DateTime.UtcNow;
            var query = _context.Bookings.AsQueryable();

            if (period.ToLower() == "daily")
            {
                var startDate = now.AddDays(-7);
                query = query.Where(b => b.CreatedAt >= startDate);
            }
            else if (period.ToLower() == "weekly")
            {
                var startDate = now.AddDays(-28);
                query = query.Where(b => b.CreatedAt >= startDate);
            }
            else if (period.ToLower() == "monthly")
            {
                var startDate = now.AddMonths(-12);
                query = query.Where(b => b.CreatedAt >= startDate);
            }

            var overview = await query
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusOverviewDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<BookingStatusOverviewDto>>.SuccessResponse(overview));
        }

        [HttpGet("active-bookings")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ActiveBookingLedgerDto>>>> GetActiveBookings()
        {
            var activeStatuses = new[] { "Pending", "PendingAdminApproval", "Approved", "Paid" };

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => activeStatuses.Contains(b.Status))
                .OrderByDescending(b => b.CreatedAt)
                .Take(20)
                .Select(b => new ActiveBookingLedgerDto
                {
                    BookingId = b.Id,
                    PropertyTitle = b.Property != null ? b.Property.Title : "Unknown",
                    BuyerName = b.FirstName + " " + b.LastName,
                    Amount = b.Property != null ? b.Property.Price : 0,
                    BookingDate = b.CreatedAt,
                    Status = b.Status
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ActiveBookingLedgerDto>>.SuccessResponse(bookings));
        }

        [HttpGet("recent-payments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecentPaymentLedgerDto>>>> GetRecentPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Include(p => p.Booking.Property)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .Select(p => new RecentPaymentLedgerDto
                {
                    PaymentId = p.Id,
                    TransactionId = p.TransactionId ?? "N/A",
                    Amount = p.Amount,
                    Date = p.CreatedAt,
                    Status = p.Status,
                    PayerName = p.Booking != null ? (p.Booking.FirstName + " " + p.Booking.LastName) : "Unknown",
                    BookingId = p.BookingId,
                    PropertyTitle = (p.Booking != null && p.Booking.Property != null) ? p.Booking.Property.Title : "Unknown"
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<RecentPaymentLedgerDto>>.SuccessResponse(payments));
        }
    }
}
