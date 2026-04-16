using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi;

public static class DiRoot {
   // Add API versioning to services
   public static IServiceCollection AddApiReaderAndVersioning(
      this IServiceCollection services
   ) {

      return services;
   }

}