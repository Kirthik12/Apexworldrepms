using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApexWorld_Backend.Common.Resilience
{
    public class ExponentialBackoffRetryPolicy
    {
        private readonly int _maxRetries;
        private readonly int _initialDelayMs;
        private readonly double _jitterFactor;
        private readonly Random _random;
        private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;

        public ExponentialBackoffRetryPolicy(
            ILogger<ExponentialBackoffRetryPolicy> logger,
            int maxRetries = 5,
            int initialDelayMs = 100,
            double jitterFactor = 0.1)
        {
            _logger = logger;
            _maxRetries = maxRetries;
            _initialDelayMs = initialDelayMs;
            _jitterFactor = jitterFactor;
            _random = new Random();
        }

        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            string operationName)
        {
            int retryCount = 0;

            while (retryCount < _maxRetries)
            {
                try
                {
                    _logger.LogInformation($"Executing {operationName}, Attempt {retryCount + 1}/{_maxRetries}");
                    return await operation();
                }
                catch (Exception ex) when (IsTransientError(ex))
                {
                    retryCount++;
                    if (retryCount >= _maxRetries)
                        throw;

                    // Exponential backoff: 2^n
                    int exponentialDelay = (int)Math.Pow(2, retryCount) * _initialDelayMs;

                    // Add jitter: random value between 0 and jitterFactor * delay
                    int jitter = (int)(_random.NextDouble() * exponentialDelay * _jitterFactor);
                    int totalDelay = exponentialDelay + jitter;

                    _logger.LogWarning($"{operationName} failed: {ex.Message}. Retrying in {totalDelay}ms (delay: {exponentialDelay}ms + jitter: {jitter}ms)");

                    await Task.Delay(totalDelay);
                }
            }

            throw new Exception($"{operationName} failed after {_maxRetries} retries");
        }

        private bool IsTransientError(Exception ex)
        {
            // Only retry transient errors
            return ex is HttpRequestException ||
                   ex is TimeoutException ||
                   ex is IOException ||
                   (ex is DbUpdateException due &&
                    due.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 40197 || sqlEx.Number == 40501 || sqlEx.Number == 40613)); // SQL timeout errors
        }
    }
}
