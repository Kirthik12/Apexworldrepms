using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApexWorld_Backend.Features.Reports.DTOs;
using ApexWorld_Backend.Features.Reports.Services;
using ApexWorld_Backend.Features.Reports.Models;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApexWorld_Backend.Modules.Reports.Controllers
{
    [Tags("Admin - Reports")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IDocumentGeneratorService _docGen;
        private readonly ApexWorld_Backend.Common.Interfaces.IRepository<Report> _reportRepo;

        public ReportsController(
            IReportService reportService, 
            IDocumentGeneratorService docGen,
            ApexWorld_Backend.Common.Interfaces.IRepository<Report> reportRepo)
        {
            _reportService = reportService;
            _docGen = docGen;
            _reportRepo = reportRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportResponseDto>>> GetReports([FromQuery] ReportFilterDto filter)
        {
            var reports = await _reportService.GetReportsAsync(filter);
            return Ok(reports);
        }

        [HttpPost]
        public async Task<ActionResult<ReportResult>> GenerateReport([FromBody] ReportRequestDto dto)
        {
            var username = User.Identity?.Name ?? "Admin";
            var result = await _reportService.GenerateReportAsync(dto, username);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportResponseDto>> GetReport(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var success = await _reportService.DeleteReportAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReportResponseDto>> UpdateReport(int id, [FromBody] ReportRequestDto dto)
        {
            var report = await _reportService.UpdateReportAsync(id, dto);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<ReportDashboardStatsDto>> GetDashboardStats()
        {
            var stats = await _reportService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("chart-data")]
        public async Task<ActionResult<ReportChartDataDto>> GetChartData([FromQuery] string period = "daily")
        {
            var chartData = await _reportService.GetChartDataAsync(period);
            return Ok(chartData);
        }

        [HttpGet("{id}/download")]
        [AllowAnonymous] // Allow direct stream down in browser window/clicks if authorized beforehand
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null || report.Status != "Completed")
            {
                return NotFound("Report not found or not completed yet.");
            }

            // Parse the saved JSON data payload dynamically into a DataTable
            System.Data.DataTable dt = new System.Data.DataTable();
            using (var doc = System.Text.Json.JsonDocument.Parse(report.DataPayload))
            {
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    bool isFirst = true;
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        if (isFirst)
                        {
                            foreach (var prop in element.EnumerateObject())
                            {
                                dt.Columns.Add(prop.Name, typeof(string));
                            }
                            isFirst = false;
                        }
                        
                        var row = dt.NewRow();
                        foreach (var prop in element.EnumerateObject())
                        {
                            row[prop.Name] = prop.Value.ToString();
                        }
                        dt.Rows.Add(row);
                    }
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        dt.Columns.Add(prop.Name, typeof(string));
                    }
                    var row = dt.NewRow();
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        row[prop.Name] = prop.Value.ToString();
                    }
                    dt.Rows.Add(row);
                }
            }

            byte[] fileBytes;
            string contentType;
            string extension;

            if (report.Format.Equals("Excel", System.StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = _docGen.GenerateExcel(dt, report.ReportName);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = "xlsx";
            }
            else if (report.Format.Equals("CSV", System.StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = _docGen.GenerateCsv(dt);
                contentType = "text/csv";
                extension = "csv";
            }
            else // Default PDF
            {
                fileBytes = _docGen.GeneratePdf(dt, report.ReportName);
                contentType = "application/pdf";
                extension = "pdf";
            }

            return File(fileBytes, contentType, $"{report.ReportName.Replace(" ", "_")}.{extension}");
        }
    }
}
