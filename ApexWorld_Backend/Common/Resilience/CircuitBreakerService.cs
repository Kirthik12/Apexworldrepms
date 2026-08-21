using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Common.Resilience
{
    public enum CircuitBreakerState
    {
        Closed,      // Normal operation
        Open,        // Reject calls
        HalfOpen     // Test recovery
    }

    public class CircuitBreakerService<TRequest, TResponse>
    {
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private int _failureCount = 0;
        private DateTime _lastFailureTime;
        private readonly int _failureThreshold = 5;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);
        private readonly ILogger _logger;

        public CircuitBreakerService(ILogger logger, int failureThreshold = 5, int timeoutMinutes = 5)
        {
            _logger = logger;
            _failureThreshold = failureThreshold;
            _timeout = TimeSpan.FromMinutes(timeoutMinutes);
        }

        public async Task<TResponse> ExecuteAsync(
            Func<TRequest, Task<TResponse>> primaryOperation,
            Func<TRequest, Task<TResponse>> fallbackOperation,
            TRequest request)
        {
            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    try
                    {
                        var response = await primaryOperation(request);
                        _failureCount = 0; // Reset on success
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _failureCount++;
                        _lastFailureTime = DateTime.UtcNow;

                        if (_failureCount >= _failureThreshold)
                        {
                            _state = CircuitBreakerState.Open;
                            _logger.LogCritical($"Circuit breaker opened. Failures: {_failureCount}. Error: {ex.Message}");
                        }

                        throw;
                    }

                case CircuitBreakerState.Open:
                    if (DateTime.UtcNow - _lastFailureTime > _timeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger.LogWarning("Circuit breaker transitioning to half-open state");
                        return await ExecuteAsync(primaryOperation, fallbackOperation, request); // Retry
                    }

                    _logger.LogWarning("Circuit breaker is open. Using fallback operation.");
                    if (fallbackOperation != null)
                        return await fallbackOperation(request);
                    throw new Exception("Circuit breaker is open and no fallback operation provided.");

                case CircuitBreakerState.HalfOpen:
                    try
                    {
                        var response = await primaryOperation(request);
                        _state = CircuitBreakerState.Closed;
                        _failureCount = 0;
                        _logger.LogInformation("Circuit breaker closed. Primary operation recovered.");
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _state = CircuitBreakerState.Open;
                        _failureCount++;
                        _lastFailureTime = DateTime.UtcNow;
                        _logger.LogWarning($"Half-open test failed. Circuit breaker reopened. Error: {ex.Message}");
                        
                        if (fallbackOperation != null)
                            return await fallbackOperation(request);
                        throw;
                    }
            }

            throw new InvalidOperationException("Unknown circuit breaker state");
        }
    }
}
