using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            // Check if the correlation ID is already present in the request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var extractedCorrelationId))
            {
                correlationId = extractedCorrelationId!;
            }
            else
            {
                // Generate a new correlation ID if not present
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Append(CorrelationIdHeaderName, correlationId);
            }

            // Add the correlation ID to the response headers for the client
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
                }
                return Task.CompletedTask;
            });

            // Push the correlation ID into the Serilog/ILogger context
            using (_logger.BeginScope("{@CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }
}
