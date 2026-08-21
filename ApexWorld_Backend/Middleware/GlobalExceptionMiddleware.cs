using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApexWorld_Backend.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An internal server error occurred. Please try again later.";

            if (exception is ConcurrencyException)
            {
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
            }
            else if (exception is System.Collections.Generic.KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
            }
            else if (exception is ArgumentException || exception.GetType().Name == "ValidationException")
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }

            context.Response.StatusCode = (int)statusCode;
            var response = ApiResponse<object>.ErrorResponse(message);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return context.Response.WriteAsync(json);
        }
    }
}

