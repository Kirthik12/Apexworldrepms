using ApexWorld.Core.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ApexWorld_Backend.Filters
{
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private const string IdempotencyHeader = "Idempotency-Key";
        private readonly IMemoryCache _cache;
        private readonly ILogger<IdempotencyFilter> _logger;

        public IdempotencyFilter(IMemoryCache cache, ILogger<IdempotencyFilter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var idempotencyKeyValues) &&
                !context.HttpContext.Request.Headers.TryGetValue("X-Razorpay-Event-Id", out idempotencyKeyValues))
            {
                // Auto-generate an idempotency key if not provided
                idempotencyKeyValues = Guid.NewGuid().ToString();
                context.HttpContext.Request.Headers[IdempotencyHeader] = idempotencyKeyValues;
            }

            var idempotencyKey = idempotencyKeyValues.ToString();
            var cacheKey = $"Idempotency_{idempotencyKey}";

            if (_cache.TryGetValue(cacheKey, out IActionResult? cachedResult))
            {
                _logger.LogWarning("Duplicate request detected for Idempotency-Key: {Key}", idempotencyKey);
                context.Result = cachedResult;
                return;
            }

            var executedContext = await next();

            if (executedContext.Exception == null && executedContext.Result != null)
            {
                // Cache the successful result for 24 hours
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                _cache.Set(cacheKey, executedContext.Result, cacheOptions);
            }
        }
    }
}
