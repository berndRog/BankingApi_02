using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi.Configure;

public sealed class SwaggerConfigureOptions(
   IApiVersionDescriptionProvider provider
) : IConfigureOptions<SwaggerGenOptions> {

   public void Configure(SwaggerGenOptions options) {
      foreach (var description in provider.ApiVersionDescriptions) {
         options.SwaggerDoc(description.GroupName, new OpenApiInfo {
            Title = "BankingApi",
            Version = description.GroupName,
            Description = description.IsDeprecated
               ? "This API version has been deprecated."
               : "Banking API as modular monolith."
         });
      }
   }
}