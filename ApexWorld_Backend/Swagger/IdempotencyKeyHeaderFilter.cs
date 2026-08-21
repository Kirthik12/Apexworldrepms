using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ApexWorld_Backend.Filters;
using System.Linq;

namespace ApexWorld_Backend.Swagger
{
    public class IdempotencyKeyHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isIdempotent = context.MethodInfo.GetCustomAttributes(true).OfType<IdempotentAttribute>().Any() ||
                               (context.MethodInfo.DeclaringType != null && context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<IdempotentAttribute>().Any());

            // if (isIdempotent)
            // {
            //     if (operation.Parameters == null)
            //     {
            //         operation.Parameters = new System.Collections.Generic.List<OpenApiParameter>();
            //     }

            //     operation.Parameters.Add(new OpenApiParameter
            //     {
            //         Name = "Idempotency-Key",
            //         In = ParameterLocation.Header,
            //         Description = "Unique key to ensure the request is processed exactly once (e.g., a GUID). If not provided, it will be automatically generated.",
            //         Required = false,
            //         Schema = new OpenApiSchema
            //         {
            //             Type = "string"
            //         }
            //     });
            // }
        }
    }
}
