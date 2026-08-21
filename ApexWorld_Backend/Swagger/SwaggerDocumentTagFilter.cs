using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace ApexWorld_Backend.Swagger
{
    public class SwaggerDocumentTagFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Tags == null || !operation.Tags.Any()) return;

            var docName = context.DocumentName.ToLower();
            
            // Remove tags that don't belong in the current document
            var tagsToRemove = operation.Tags.Where(t => 
                (docName == "public" && (t.Name.StartsWith("Admin", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("Buyer", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("SubAdmin", System.StringComparison.OrdinalIgnoreCase))) ||
                (docName == "admin" && (t.Name.StartsWith("Public", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("Buyer", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("SubAdmin", System.StringComparison.OrdinalIgnoreCase))) ||
                (docName == "subadmin" && (t.Name.StartsWith("Public", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("Buyer", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("Admin", System.StringComparison.OrdinalIgnoreCase))) ||
                (docName == "buyer" && (t.Name.StartsWith("Public", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("Admin", System.StringComparison.OrdinalIgnoreCase) || t.Name.StartsWith("SubAdmin", System.StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            foreach (var tag in tagsToRemove)
            {
                operation.Tags.Remove(tag);
            }
            
            // If all tags were removed (which shouldn't happen based on our inclusion predicate, but just in case)
            if (!operation.Tags.Any())
            {
                operation.Tags.Add(new OpenApiTag { Name = "Uncategorized" });
            }
        }
    }
}
