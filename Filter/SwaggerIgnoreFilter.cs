using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using MultiTenantApi.Attributes;

namespace MultiTenantApi.Filters
{
    public class SwaggerIgnoreFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var routesToRemove = context.ApiDescriptions
                .Where(apiDesc =>
                    apiDesc.ActionDescriptor.EndpointMetadata
                        .Any(meta => meta.GetType() == typeof(SwaggerIgnoreAttribute)))
                .Select(apiDesc => "/" + apiDesc.RelativePath.TrimEnd('/'))
                .ToList();

            foreach (var path in routesToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }
        }
    }
}
