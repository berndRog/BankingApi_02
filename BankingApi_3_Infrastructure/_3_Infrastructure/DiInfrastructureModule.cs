using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.ReadModel;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure._3_Security;
using BankingApi._3_Infrastructure._5_Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApi._3_Infrastructure;

public static class DiInfrastructureModule {
   
   public static IServiceCollection AddInfrastructureModule(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      
      var connectionString = configuration.GetConnectionString("BankingApiDb");
      Console.WriteLine("---> Using SQLite connection string: " + connectionString);
      
      services.AddDbContext<AppDbContext>(options =>
         options.UseSqlite(connectionString)
      );

      // BC Db Contexts
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>(); 
      services.AddScoped<IAccountDbContext, AccountDbContextEf>(); 
      
      // Adapters
      services.AddScoped<ICustomerContract, CustomerContractEf>();
      services.AddScoped<IAccountContract, AccountContractEf>();
      
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      services.AddScoped<IAccountRepository, AccountRepositoryEf>();
      
      // ReadModels
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();  
      services.AddScoped<IAccountReadModel, AccountReadModelEf>();  
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      // IdentityGateway
      //services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();
      services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway(
         subject: "11111111-0002-0000-0000-000000000000",
         username: "w.wagner@banking.de",
         createdAt: DateTimeOffset.UtcNow,
         adminRights: 511
      ));
      
      
      // IClock
      services.AddScoped<IClock, BankingSystemClock>();
      
      return services;
   }
}