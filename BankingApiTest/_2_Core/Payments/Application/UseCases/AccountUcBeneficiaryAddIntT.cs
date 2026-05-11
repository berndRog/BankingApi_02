using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Payments.Application.UseCases;

public sealed class AccountUcBeneficiaryAddIntT : TestBaseIntegration {

   public AccountUcBeneficiaryAddIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "AccountUcBeneficiaryAddIntTTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task AddBeneficiaryUt() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcBeneficiaryAdd>();
      
      // Arrange
      var account = seed.Account1();
      accountRepository.Add(account);
      await unitOfWork.SaveAllChangesAsync("Add Account1", ct);
      unitOfWork.ClearChangeTracker();
      
      // Act
      var beneficiary = seed.Beneficiary1();
      var beneficiary1Dto = beneficiary.ToBeneficiaryDto();
      var result = await sut.ExecuteAsync(
         accountId: account.Id,
         beneficiaryDto: beneficiary1Dto,
         ct: ct);
      True(result.IsSuccess);

      // Assert
      var actualAccount = 
         await accountRepository.FindByIdWithBeneficiariesAsync(account.Id, ct);
      NotNull(actualAccount);
      var actual = actualAccount.Beneficiaries
          .FirstOrDefault(b => b.Id == beneficiary.Id);
      NotNull(actual);
      Equal(beneficiary.AccountId, actual.AccountId);
      Equal(beneficiary.Name, actual.Name);
      Equal(beneficiary.IbanVo, actual.IbanVo);
   }
   
}