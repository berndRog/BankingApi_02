using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcCreateIntT : TestBaseIntegration {
   
   public AccountUcCreateIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "AccountUcCreateIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task Create_account_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcCreate>();
      
      // Arrange
      // Employee2 is used as Admin 
      var employee2Id = Guid.Parse("00000000-0002-0000-0000-000000000000");
     
      // Customer 1 is the owner of Account1
      var customer = seed.Customer1();
      customerRepository.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Seeding data", ct);
      unitOfWork.ClearChangeTracker(); 
      
      var account = seed.Account1();
      var accountDto = account.ToAccountDto();
      
      // Act
      var resultAccountCreate = await sut.ExecuteAsync(
         customerId: customer.Id,
         accountDto: accountDto,
         ct: ct
      );
      True(resultAccountCreate.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actual = await accountRepository.FindByIdAsync(account.Id, ct);
      NotNull(actual);
      Equal(account.Id, actual!.Id);
      Equal(account.IbanVo, actual.IbanVo);
      Equal(account.BalanceVo, actual.BalanceVo);
      Equal(employee2Id, actual.CreatedByEmployeeId);
      Null(actual.DeactivatedByEmployeeId);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_iban_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var owner = seed.Customer1();
      var account = seed.Account1();
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcCreate>();
      var accountDto = account.ToAccountDto();

      // Act
      accountDto = accountDto with { Iban = "ABC123456789" };
      var result = await sut.ExecuteAsync(
         customerId: owner.Id, 
         accountDto: accountDto,
         ct: ct
      );
      True(result.IsFailure);
   }
   
   
}