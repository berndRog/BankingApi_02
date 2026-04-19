using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi.Configure;

public sealed class SwaggerNormalizeResponseContentTypesFilter: IOperationFilter {

   public void Apply(OpenApiOperation operation, OperationFilterContext context) {
      foreach (var response in operation.Responses) {
         if (IsNoContentResponse(response.Key))
            continue;

         if (IsProblemDetailsResponse(response.Key, response.Value)) {
            KeepOnlyContentType(response.Value, "application/problem+json");
            continue;
         }

         if (IsSuccessResponse(response.Key))
            KeepOnlyContentType(response.Value, "application/json");
      }
   }

   private static bool IsSuccessResponse(string statusCode) =>
      int.TryParse(statusCode, out var code) && code is >= 200 and < 300;

   private static bool IsNoContentResponse(string statusCode) =>
      statusCode is "204" or "304";

   private static bool IsProblemDetailsResponse(string statusCode, OpenApiResponse response) =>
      response.Content.Count > 0 &&
      (IsClientOrServerErrorResponse(statusCode) ||
       response.Content.Values.Any(mediaType =>
          mediaType.Schema.Reference?.Id == nameof(Microsoft.AspNetCore.Mvc.ProblemDetails)));

   private static bool IsClientOrServerErrorResponse(string statusCode) =>
      int.TryParse(statusCode, out var code) && code >= 400;

   private static void KeepOnlyContentType(OpenApiResponse response, string contentType) {
      if (response.Content.Count == 0)
         return;

      var preferredContent = response.Content.TryGetValue(contentType, out var existingContent)
         ? existingContent
         : response.Content.Values.First();

      response.Content.Clear();
      response.Content[contentType] = preferredContent;
   }
}
