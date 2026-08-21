using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Constants;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ApexWorld_Backend.Features.Dashboard.Controllers
{
    [Tags("Admin - Dashboard Metrics")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardMetricsController : ControllerBase
    {
        private readonly IRepository<DashboardMetric> _repository;

        public DashboardMetricsController(IRepository<DashboardMetric> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> GetAllMetrics()
        {
            var metrics = await _repository.GetAllAsync();
            return Ok(ApiResponse<object>.SuccessResponse(metrics));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> GetMetricById(int id)
        {
            var metric = await _repository.GetByIdAsync(id);
            if (metric == null) return NotFound(ApiResponse<string>.ErrorResponse("Metric not found"));
            return Ok(ApiResponse<object>.SuccessResponse(metric));
        }

        [HttpPost]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> CreateMetric([FromBody] DashboardMetric metric)
        {
            var createdMetric = await _repository.AddAsync(metric);
            return Ok(ApiResponse<object>.SuccessResponse(createdMetric));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> UpdateMetric(int id, [FromBody] DashboardMetric metricUpdate)
        {
            var existingMetric = await _repository.GetByIdAsync(id);
            if (existingMetric == null) return NotFound(ApiResponse<string>.ErrorResponse("Metric not found"));

            existingMetric.Key = metricUpdate.Key;
            existingMetric.Value = metricUpdate.Value;
            existingMetric.Category = metricUpdate.Category;
            existingMetric.Trend = metricUpdate.Trend;
            existingMetric.DisplayName = metricUpdate.DisplayName;

            await _repository.UpdateAsync(existingMetric);
            return Ok(ApiResponse<object>.SuccessResponse(existingMetric));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> DeleteMetric(int id)
        {
            var existingMetric = await _repository.GetByIdAsync(id);
            if (existingMetric == null) return NotFound(ApiResponse<string>.ErrorResponse("Metric not found"));

            await _repository.DeleteAsync(existingMetric);
            return Ok(ApiResponse<string>.SuccessResponse("Metric deleted successfully"));
        }

        [HttpGet("filter")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> FilterMetrics([FromQuery] string? category, [FromQuery] string? trend)
        {
            var allMetrics = await _repository.GetAllAsync();
            var query = allMetrics.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(trend))
            {
                query = query.Where(m => m.Trend.Equals(trend, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(ApiResponse<object>.SuccessResponse(query.ToList()));
        }
    }
}
