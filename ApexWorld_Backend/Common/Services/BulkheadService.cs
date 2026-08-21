using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Common.Services
{
    public interface IBulkheadService
    {
        Task<T> ExecuteAsync<T>(string poolName, Func<Task<T>> action);
        Task ExecuteAsync(string poolName, Func<Task> action);
    }

    public class BulkheadService : IBulkheadService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _pools = new();

        public async Task<T> ExecuteAsync<T>(string poolName, Func<Task<T>> action)
        {
            var semaphore = GetSemaphore(poolName);
            await semaphore.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task ExecuteAsync(string poolName, Func<Task> action)
        {
            var semaphore = GetSemaphore(poolName);
            await semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        private SemaphoreSlim GetSemaphore(string poolName)
        {
            return _pools.GetOrAdd(poolName, name => 
            {
                // Different pools can have different limits based on name
                int limit = name switch
                {
                    "Payment" => 10,  // Max 10 concurrent payments
                    "Booking" => 20,  // Max 20 concurrent bookings
                    "Loan" => 15,     // Max 15 concurrent loans
                    _ => 10
                };
                return new SemaphoreSlim(limit, limit);
            });
        }
    }
}
