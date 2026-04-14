using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi;

public static class DiRoot {
   // Add API versioning to services
   public static IServiceCollection AddApiReaderAndVersioning(
      this IServiceCollection services
   ) {
      var apiVersionReader = ApiVersionReader.Combine(
         new UrlSegmentApiVersionReader()
         // new HeaderApiVersionReader("x-api-version")
         // new MediaTypeApiVersionReader("x-api-version"),
         // new QueryStringApiVersionReader("api-version")
      );

      services.AddApiVersioning(options => {
         options.DefaultApiVersion = new ApiVersion(1, 0);
         options.AssumeDefaultVersionWhenUnspecified = true;
         options.ReportApiVersions = true;
         //          opt.ApiVersionReader = new UrlSegmentApiVersionReader();
         options.ApiVersionReader = apiVersionReader;
      })
      .AddMvc()
      .AddApiExplorer(options => {
         options.GroupNameFormat = "'v'VVV";
         options.SubstituteApiVersionInUrl = true;
      });

      return services;
   }

   // Add Swagger/OpenAPI to services
   public static IServiceCollection AddSwagger(
      this IServiceCollection services
   ) {
      services.AddEndpointsApiExplorer();

      // create SwaggerDoc(...) dynamically for all discovered API versions
      services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

      services.AddSwaggerGen(options => {
         // include XML docs from all copied XML files in output folder
         var basePath = AppContext.BaseDirectory;

         foreach (var xmlFile in Directory.EnumerateFiles(basePath, "*.xml")) {
            options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
         }

         // optional: nicer schema names in larger modular solutions
         options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

         // optional: remove version parameter from generated operation parameters
         options.OperationFilter<SwaggerRemoveVersionParameterFilter>();

         // optional: replace version placeholder in route templates
         options.DocumentFilter<SwaggerReplaceVersionWithExactValueInPathFilter>();
      });

      return services;
   }
}