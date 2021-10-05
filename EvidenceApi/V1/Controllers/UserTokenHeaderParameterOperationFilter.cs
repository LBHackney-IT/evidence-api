using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EvidenceApi.V1.Controllers
{
    public class UserTokenHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "UserEmail",
                In = ParameterLocation.Header,
                Description = "Hackney email address of the user accessing the endpoint",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
