using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApi._2_Core.Customers;

public static class DiCustomerModules {
   
   public static IServiceCollection AddCustomerModule(
      this IServiceCollection services
   ) {
      // Inbound ports / Use Cases
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcDeactivate>();
      services.AddScoped<CustomerUcUpdate>();
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();
      return services;
   }
}