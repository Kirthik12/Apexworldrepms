using ApexWorld_Backend.Features.Audit.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Audit.Services{
    public interface IAuditQueryService
    {
        Task<(IEnumerable<AuditDto> Items, int TotalCount)> GetAuditLogsAsync(int pageNumber, int pageSize);
    }
}



