using System.Transactions;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class AccountRepositoryIntTests : TestBaseIntegration {
   
   public AccountRepositoryIntTests() {
      DbMode = DbMode.FileUnique;
      DbName = "BankingTest";
      SensitiveDataLogging = true;
   }
   
   #region --- Aggregate root: Account ------------------------------------------------------
   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.Accounts;
      var account1 = accounts[0];
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actual = await repository.FindByIdAsync(account1.Id, ct);
      
      NotNull(actual);
      Equal(account1.Id, actual.Id);
      Equal(account1.IbanVo, actual.IbanVo);
      Equal(account1.BalanceVo, actual.BalanceVo);
      Equal(account1.CustomerId, actual.CustomerId);
   }
   
   [Fact]
   public async Task FindByIbanAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.Accounts;
      var account1 = accounts[0];
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actual = await repository.FindByIbanAsync(account1.IbanVo, ct);
      
      NotNull(actual);
      Equal(account1.Id, actual.Id);
      Equal(account1.IbanVo, actual.IbanVo);
      Equal(account1.BalanceVo, actual.BalanceVo);
      Equal(account1.CustomerId, actual.CustomerId);
   }
   
   [Fact]
   public async Task ExistsByOwnerIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.Accounts;
      var customer1 = seed.Customer1();
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actual = await repository.ExistsByCustomerIdAsync(customer1.Id, ct);
      
      True(actual);
   }
   
   [Fact]
   public async Task SelelctByCustomerIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.Accounts;
      var customer1 = seed.Customer1();
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var expectedIds = accounts
         .Where(a => a.CustomerId == customer1.Id)
         .Select(a => a.Id)
         .OrderBy(a => a)
         .ToList();

      // Act
      var actual = await repository.SelelctByCustomerIdAsync(customer1.Id, ct);
      var actualIds = actual
         .Select(a => a.Id)
         .OrderBy(id => id)
         .ToList();
      
      NotNull(actual);
      // Compare ids
      Equals(expectedIds, actualIds);
   }
   
   [Fact]
   public async Task FindAccountWithBeneficiaryByIdAsync() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var accounts = seed.AddBeneficiariesToAccounts().ToList();
      repository.AddRange(accounts);
      await unitOfWork.SaveAllChangesAsync("Accounts inserted", ct);
      unitOfWork.ClearChangeTracker();

      var account1 = accounts[0];
      var beneficiary1 = seed.Beneficiary1();
      
      var expectedBeneficiary = accounts
         .Where(a => a.Id == account1.Id)
         .SelectMany(a => a.Beneficiaries)
         .SingleOrDefault(b => b.Id == beneficiary1.Id);
      
      // Act  (load account with account1.Id and the beneficiary with beneficiary1.Id
      var actual = await repository
         .FindAccountByWithBeneficiaryByIdAsync(account1.Id, beneficiary1.Id, ct);
      
      // Assert
      NotNull(actual);
      Equal(account1.Id, actual.Id);
      Equal(account1.IbanVo, actual.IbanVo);
      Equal(account1.BalanceVo, actual.BalanceVo);
      Equal(account1.CustomerId, actual.CustomerId);
      Single(actual.Beneficiaries);
      var actualBeneficiary = actual.Beneficiaries.ToList()[0];
      Equal(expectedBeneficiary?.Id, actualBeneficiary?.Id);
      Equal(expectedBeneficiary?.Name, actualBeneficiary?.Name);
      Equal(expectedBeneficiary?.IbanVo, actualBeneficiary?.IbanVo);   
   }
   #endregion
   
}