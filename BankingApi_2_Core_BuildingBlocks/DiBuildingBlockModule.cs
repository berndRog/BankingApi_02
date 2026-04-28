using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._3_FakeInfraStructure._2_Persitence.Adapters;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApi;

public static class DiBuildingBlocksModule {
   
   public static IServiceCollection AddBuildingBlocksModule(
      this IServiceCollection services
   ) {
      // Fake implmentation or Contract
      services.AddScoped<IEmployeeContract, FakeEmployeeContractEf>();
      return services;
   }
}