using System.Data.Common;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.ReadModel;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApiTest._3_Infrastructure._3_Security;
using BankingApiTest._3_Infrastructure._5_Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest.TestInfrastructure;

public static class DiTestModules {
   
   public static IServiceCollection AddTestModules(
      this IServiceCollection services,
      DbConnection dbConnection,
      bool enableSensitiveDataLogging = true
   ) {
      services.AddSingleton(dbConnection);

      services.AddDbContext<AppDbContext>((sp, options) => {
         var connection = sp.GetRequiredService<DbConnection>();
         options.UseSqlite(connection);

         if (enableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();
      });

      // BC Db Contexts
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>();
      services.AddScoped<IAccountDbContext, AccountDbContextEf>();
      
      // Contracts
      services.AddScoped<ICustomerContract, CustomerContractEf>();
      services.AddScoped<IAccountContract, AccountContractEf>();
      
      // Readmodels
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();
      services.AddScoped<IAccountReadModel, AccountReadModelEf>();
      
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      services.AddScoped<IAccountRepository, AccountRepositoryEf>();
     
      // Customer UseCases
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcDeactivate>();
      
      // Account UseCases
      services.AddScoped<IAccountUseCases, AccountUseCases>();
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      // Clock 
      services.AddSingleton<IClock>(_ => new FakeClock(FakeClock.DefaultUtcNow));
      
      
      // IdentityGateway = CustomerRegister() from Seed
      // simulate loggedin customer
      services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway(
         subject: "70000000-0007-0000-0000-000000000000",
         username: "e.engel@freenet.de",
         createdAt: FakeClock.DefaultUtcNow,
         adminRights: 0
      ));
      
      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();
      
      return services;
   }
}