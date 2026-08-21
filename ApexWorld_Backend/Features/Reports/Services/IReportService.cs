using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Reports.DTOs;

namespace ApexWorld_Backend.Features.Reports.Services{
    public interface IReportService
    {
        Task<ReportResult> GenerateReportAsync(ReportRequestDto requestDto, string generatedBy = "Admin");
        Task<IEnumerable<ReportResponseDto>> GetReportsAsync(ReportFilterDto filter);
        Task<ReportDashboardStatsDto> GetDashboardStatsAsync();
        Task<ReportResponseDto?> GetReportByIdAsync(int id);
        Task<bool> DeleteReportAsync(int id);
        Task<ReportResponseDto?> UpdateReportAsync(int id, ReportRequestDto dto);
        Task<ReportChartDataDto> GetChartDataAsync(string period);
    }
}
