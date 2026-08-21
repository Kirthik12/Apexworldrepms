using ApexWorld_Backend.Features.Audit.Services;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Audit.Controllers
{
    [Tags("Admin - Audit Logs")]
[Authorize(Roles = Roles.Admin)]
    public class AuditController : ControllerBase
    {
        private readonly IAuditQueryService _auditQueryService;

        public AuditController(IAuditQueryService auditQueryService)
        {
            _auditQueryService = auditQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var (items, totalCount) = await _auditQueryService.GetAuditLogsAsync(pageNumber, pageSize);

            var response = new
            {
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };

            return Ok(ApiResponse<object>.SuccessResponse(response, "Audit logs retrieved successfully."));
        }
    }
}




