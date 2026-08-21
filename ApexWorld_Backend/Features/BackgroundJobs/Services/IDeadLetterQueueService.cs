using ApexWorld_Backend.Features.BackgroundJobs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.BackgroundJobs.Services
{
    public interface IDeadLetterQueueService
    {
        Task EnqueueAsync(DeadLetterMessage message);
        Task<IEnumerable<DeadLetterMessage>> GetUnresolvedMessagesAsync();
        Task<DeadLetterMessage?> GetMessageAsync(int id);
        Task MarkAsResolvedAsync(int id);
        Task ProcessDeadLetterQueueAsync();
    }
}
