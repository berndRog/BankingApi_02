using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcDeactivateIntT : TestBaseIntegration {
   
   public AccountUcDeactivateIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "AccountUcDeactivateIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task Deactivate_Account_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None; 
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcDeactivate>();
      
      // Arrange
      // Employee2 is used as Admin 
      var employee2Id = Guid.Parse("00000000-0002-0000-0000-000000000000");

      var account = seed.Account1();
      accountRepository.Add(account);
      await unitOfWork.SaveAllChangesAsync("Add Account1", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var resultAccountDeactivate = await sut.ExecuteAsync(
         accountId: account.Id,
         ct: ct
      );
      True(resultAccountDeactivate.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actual = await accountRepository.FindByIdAsync(account.Id, ct);
      NotNull(actual);
      Equal(account.Id, actual!.Id);
      Equal(account.IbanVo, actual.IbanVo);
      Equal(account.BalanceVo, actual.BalanceVo);
      Equal(employee2Id, actual.CreatedByEmployeeId);
      Equal(employee2Id, actual.DeactivatedByEmployeeId);
   }
   
}