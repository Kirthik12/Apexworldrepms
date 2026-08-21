using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.BackgroundJobs.Models;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.BackgroundJobs.Services
{
    public class DeadLetterQueueService : IDeadLetterQueueService
    {
        private readonly IRepository<DeadLetterMessage> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobs;

        public DeadLetterQueueService(IRepository<DeadLetterMessage> repository, IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobs)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
        }

        public async Task EnqueueAsync(DeadLetterMessage message)
        {
            await _repository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<DeadLetterMessage>> GetUnresolvedMessagesAsync()
        {
            return await _repository.GetAsync(m => !m.IsResolved);
        }

        public async Task<DeadLetterMessage?> GetMessageAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task MarkAsResolvedAsync(int id)
        {
            var msg = await _repository.GetByIdAsync(id);
            if (msg != null)
            {
                msg.IsResolved = true;
                await _repository.UpdateAsync(msg);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ProcessDeadLetterQueueAsync()
        {
            var unresolved = await _repository.GetAsync(m => !m.IsResolved && m.RetryCount < 5);
            foreach (var msg in unresolved)
            {
                msg.RetryCount++;
                if (msg.RetryCount >= 5)
                {
                    msg.IsResolved = true;
                    msg.Exception = "Max retries exceeded. " + msg.Exception;
                }
                else
                {
                    // Simulated retry logic. In a real system, we'd dynamically invoke the original queue processor based on payload.
                    _backgroundJobs.Enqueue(() => Console.WriteLine($"Auto-Retrying DLQ Message {msg.Id} (Attempt {msg.RetryCount}/5)"));
                }
                await _repository.UpdateAsync(msg);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
