using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class AccountReadModelIntT : TestBaseIntegration {

   public AccountReadModelIntT() {
      DbName = nameof(AccountReadModelIntT);
      DbMode = DbMode.FileUnique;
      SensitiveDataLogging = true;
   }
      
   
   #region --- Aggregate root: Account ------------------------------------------------------
   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.Accounts;
      var accountDtos = accounts.Select(a => a.ToAccountDto()).ToList();
      var account1 = accounts[0];
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(account1.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value;
      NotNull(actual);
      Equals(accountDtos, actual);
   }
   
   
   [Fact]
   public async Task FindByIbanAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var accounts = seed.Accounts;
      var accountDtos = accounts.Select(a => a.ToAccountDto()).ToList();
      var account1Dto = accounts[0].ToAccountDto();
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();


      // Act
      var result = await readModel.FindByIbanAsync(account1Dto.Iban, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDto = result.Value;
      NotNull(actualDto);
      Equal(account1Dto.Id, actualDto.Id);
      Equal(account1Dto.Iban, actualDto.Iban);
      Equal(account1Dto.Balance, actualDto.Balance);
      Equal(account1Dto.CustomerId, actualDto.CustomerId);
   }
   
   [Fact]
   public async Task SelectByCustomerIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var accounts = seed.Accounts;
      accountRepository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Customers&Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var customer1 = seed.Customer1();
      var expectedAccountDtos = accounts
         .Where(a => a.CustomerId == customer1.Id)
         .Select(a => a.ToAccountDto())
         .ToList();
      
      // Act
      var result = await readModel.SelectByCustomerIdAsync(customer1.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDtos = result.Value;
      NotNull(actualDtos);
      Equals(expectedAccountDtos, actualDtos);
   }
   #endregion
   
   [Fact]
   public async Task FindBeneficiariesById_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var accounts = seed.AddBeneficiariesToAccounts().ToList();
      accountRepository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Customers&Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var account1 = accounts[0];
      var beneficiary1 = seed.Beneficiary1();
      var expectedBeneficiaryDto = beneficiary1.ToBeneficiaryDto();
      
      // Act
      var result = await readModel.FindBeneficiaryByIdAsync(account1.Id, beneficiary1.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDto = result.Value;
      NotNull(actualDto);
      Equals(expectedBeneficiaryDto, actualDto);
   }
   
   [Fact]
   public async Task SelectBeneficiariesByAccountIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var accounts = seed.AddBeneficiariesToAccounts();
      accountRepository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Customers&Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var account1 = seed.Account1();
      var expectedBeneficiaryDtos = account1.Beneficiaries
         .Select(b => b.ToBeneficiaryDto())
         .ToList();
      
      // Act
      var result = await readModel.SelectBeneficiariesByAccountIdAsync(account1.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDtos = result.Value;
      NotNull(actualDtos);
      Equals(expectedBeneficiaryDtos, actualDtos);
   }

   [Fact]
   public async Task SelectBeneficiariesByNameIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAccountReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var accounts = seed.AddBeneficiariesToAccounts();
      accountRepository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Customers&Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var account1 = seed.Account1();
      var expectedBeneficiaryDtos = account1.Beneficiaries
         .Select(b => b.ToBeneficiaryDto())
         .ToList();
      
      // Act
      var result = await readModel.SelectBeneficiariesByNameAsync(account1.Id, "Conrad",ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDtos = result.Value;
      NotNull(actualDtos);
      Equals(expectedBeneficiaryDtos, actualDtos);
   }

  
}