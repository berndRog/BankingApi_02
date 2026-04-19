using Asp.Versioning.ApiExplorer;
using BankingApi._2_Core.Customers;
using BankingApi._2_Core.Payments;
using BankingApi._3_Infrastructure;
using BankingApi.Configure;
namespace BankingApi;

public class Program {
   
   public static async Task Main(string[] args) {
      
      //---- Configure DI Container (IServiceCollection) ----
      var builder = WebApplication.CreateBuilder(args);
      
      // Configure Logging Providers & Http Logging       
      ConfigureLoggingAndHttpLogging.Configure(builder);
  
      // Access Http-Request in Infrastructure
      builder.Services.AddHttpContextAccessor();
      
      // Controllers
      builder.Services.AddControllers();

      // Modules
      builder.Services.AddCustomerModule();
      builder.Services.AddPaymentModule();
      builder.Services.AddInfrastructureModule(builder.Configuration);

      // Add Error handling
      builder.Services.AddProblemDetails();
      
      // AuthN (Bearer) + AuthZ
      // builder.Services.AddAuthNAuthZ(builder.Configuration);
      
      // API versioning 
      builder.Services.AddApiReaderAndVersioning();
      
      // Swagger
      builder.Services.AddSwagger();
      
      //---- Create App and Setup Middleware pipeline ----
      var app = builder.Build();

      
      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
           
         app.UseHttpLogging();
         app.UseDeveloperExceptionPage();
         
         app.UseSwagger();

         var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

         app.UseSwaggerUI(options => {
            foreach (var description in provider.ApiVersionDescriptions) {
               options.SwaggerEndpoint(
                  $"/swagger/{description.GroupName}/swagger.json",
                  $"BankingApi {description.GroupName.ToUpperInvariant()}"
               );
            }
            options.RoutePrefix = "swagger";
         });
      }
      
      app.UseHttpsRedirection();

      //app.UseAuthentication();
      //app.UseAuthorization();

      app.MapControllers();

      await app.RunAsync();
   }
}