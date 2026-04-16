using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcBeneficiaryRemoveIntT : TestBaseIntegration {

   public AccountUcBeneficiaryRemoveIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "AccountUcBeneficiaryRemoveIntTTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task AddBeneficiaryUt() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var accountUcBeneficiaryAdd = scope.ServiceProvider.GetRequiredService<AccountUcBeneficiaryAdd>();
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcBeneficiaryRemove>();
      
      // Arrange
      var account = seed.Account1();
      accountRepository.Add(account);
      await unitOfWork.SaveAllChangesAsync("Add Account1", ct);
      unitOfWork.ClearChangeTracker();
      
      var beneficiary = seed.Beneficiary1();
      var beneficiary1Dto = beneficiary.ToBeneficiaryDto();
      var resultAdd = await accountUcBeneficiaryAdd.ExecuteAsync(
         accountId: account.Id,
         beneficiaryDto: beneficiary1Dto,
         ct: ct);
      True(resultAdd.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Act
      var resultRemove = await sut.ExecuteAsync(
         accountId: account.Id,
         beneficiaryId: beneficiary.Id,
         ct: ct);
      True(resultRemove.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actualAccount = 
         await accountRepository.FindAccountByIdWithBeneficiariesAsync(account.Id, ct);
      NotNull(actualAccount);
      var actual = actualAccount.Beneficiaries
          .FirstOrDefault(b => b.Id == beneficiary.Id);
      Null(actual);
   }
}