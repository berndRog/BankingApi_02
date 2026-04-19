using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi.Configure;

public sealed class SwaggerProblemDetailsSchemaFilter: ISchemaFilter {

   public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
      if (context.Type != typeof(ProblemDetails))
         return;

      schema.AdditionalPropertiesAllowed = true;
      schema.Properties["code"] = new OpenApiSchema {
         Type = "string",
         Nullable = true
      };
      schema.Properties["traceId"] = new OpenApiSchema {
         Type = "string",
         Nullable = true
      };
   }
}
