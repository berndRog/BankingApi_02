using Microsoft.AspNetCore.HttpLogging;
namespace BankingApi;

public static class ConfigureLoggingAndHttpLogging {
   
   public static void Configure(WebApplicationBuilder builder) {
      // Configure Logging Providers
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.AddDebug();

      // Configure Http Logging                                                                                                             
      builder.Services.AddHttpLogging(o => {
         o.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.ResponseHeaders;

         // optional: Bodies (nur DEV, Achtung: kann sensibel sein)
         o.LoggingFields |=
            HttpLoggingFields.RequestBody |
            HttpLoggingFields.ResponseBody;

         // Body limits (avoid huge logs)
         o.RequestBodyLogLimit = 1024;
         o.ResponseBodyLogLimit = 4096;
         // Allow-list only non-sensitive headers you actually want.
         o.ResponseHeaders.Clear();
         o.ResponseHeaders.Add("Content-Type");
         o.RequestHeaders.Add("Accept");

         // Force redaction for common sensitive headers (even if someone adds them later).
         o.RequestHeaders.Add("Authorization");
         //o.RequestHeaders.Add("Cookie");
         o.RequestHeaders.Add("Origin");
         o.RequestHeaders.Add("Referer");
         o.RequestHeaders.Add("Set-Cookie");

         o.MediaTypeOptions.AddText("application/json");
         o.MediaTypeOptions.AddText("application/json");
         o.MediaTypeOptions.AddText("application/problem+json");
         o.MediaTypeOptions.AddText("application/*+json");
         o.MediaTypeOptions.AddText("text/plain");
      });
   }
}