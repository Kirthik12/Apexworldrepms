using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

namespace ApexWorld_Backend.Common.Resilience
{
    public static class ResiliencePolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetExponentialBackoffRetryPolicy(int maxRetries = 5, int initialDelayMs = 100, double jitterFactor = 0.1)
        {
            var random = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    maxRetries,
                    retryAttempt => 
                    {
                        int exponentialDelay = (int)Math.Pow(2, retryAttempt) * initialDelayMs;
                        int jitter = (int)(random.NextDouble() * exponentialDelay * jitterFactor);
                        return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
                    },
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"[Polly] Retry {retryAttempt} for {outcome.Result?.RequestMessage?.RequestUri}. Delaying {timespan.TotalMilliseconds}ms. {(outcome.Exception != null ? outcome.Exception.Message : $"Status code: {outcome.Result?.StatusCode}")}");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int exceptionsAllowedBeforeBreaking = 5, int durationOfBreakMinutes = 5)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromMinutes(durationOfBreakMinutes),
                    onBreak: (outcome, breakDelay) =>
                    {
                        Console.WriteLine($"[Polly] Circuit breaker opened for {breakDelay.TotalMinutes} minutes. {(outcome.Exception != null ? outcome.Exception.Message : $"Status code: {outcome.Result?.StatusCode}")}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("[Polly] Circuit breaker reset.");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("[Polly] Circuit breaker half-open (testing next call).");
                    });
        }
    }
}
