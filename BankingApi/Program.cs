using Asp.Versioning.ApiExplorer;
using BankingApi._2_Core.Customers;
using BankingApi._2_Core.Payments;
using BankingApi._3_Infrastructure;
namespace BankingApi;

public class Program {
   
   public static async Task Main(string[] args) {
      
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
      
      var app = builder.Build();

      
      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
           
         app.UseHttpLogging();
         app.UseDeveloperExceptionPage();
      }
      
      app.UseHttpsRedirection();

      //app.UseAuthentication();
      //app.UseAuthorization();

      app.MapControllers();

      await app.RunAsync();
   }
}