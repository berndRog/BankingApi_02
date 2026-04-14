using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Core.Domain.Aggregates;

public sealed class AccountUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;

   private readonly Customer _customer;
   private readonly Account _account;
   private readonly Guid _employee2Id;
   
   public AccountUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      _customer = _seed.Customer1();
      _account = _seed.Account1();
      _employee2Id = Guid.Parse(_seed.Employee2Id);
   }

   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Arrange
      // Act
      var result = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: _account.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Account>(actual);
      NotEqual(Guid.Empty, actual.Id);
      Equal(_account.Id, actual.Id);
      Equal(_account.IbanVo, actual.IbanVo);
      Equal(_account.BalanceVo, actual.BalanceVo);
      Equal(_customer.Id, actual.CustomerId);
   }

   [Fact]
   public void Create_without_id_generates_new_id() {
      // Act
      var result = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: null
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      NotEqual(Guid.Empty, actual.Id);
      Equal(_account.IbanVo, actual.IbanVo);
      Equal(_account.BalanceVo, actual.BalanceVo);
      Equal(_customer.Id, actual.CustomerId);
   }

   [Fact]
   public void Create_with_invalid_id_fails() {
      // Act
      var result = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: "not-a-guid"
      );
      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("DE10 1000 0000 0000 0000 00")] // wrong checksum per your conversation history
   [InlineData("XX00 0000 0000 0000 0000 00")] // unknown country
   public void Create_with_invalid_iban_fails(string iban) {
      // Act
      var result = IbanCheck.Run(iban);
  
      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Fact]
   public void Create_with_notallowed_customerId_is_failure() {
      // Act
      var result = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: "not.allowed"
      );
      // Assert
      True(result.IsFailure);
   }
   
   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: _account.Id.ToString()
      );

      var result2 = Account.Create(
         customerId: _customer.Id,
         ibanVo: _account.IbanVo,
         balanceVo: _account.BalanceVo,
         createdByEmployeeId: _employee2Id,
         createdAt: _account.CreatedAt,
         id: _account.Id.ToString()
      );

      True(result1.IsSuccess);
      True(result2.IsSuccess);
      Equal(result1.Value!.Id, result2.Value!.Id);
      Equal(result1.Value!.IbanVo, result2.Value!.IbanVo);
      Equal(result1.Value!.CustomerId, result2.Value!.CustomerId);
      Equal(result1.Value!.BalanceVo, result2.Value!.BalanceVo);
   }
 
   #region --- Beneficiaries ----------------------------------------------------------------
   [Fact]
   public void AddBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1();
      var beneficiary = _seed.Beneficiary1();
      
      // Act
      account.AddBeneficiary(
         beneficiary: beneficiary,
         updatedAt: _clock.UtcNow
      );
      
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary.Id);
      NotNull(actual);
      Equal(beneficiary, actual);
   }
   [Fact]
   public void RemoveBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1();
      var beneficiary1 = _seed.Beneficiary1();
      var beneficiary2 = _seed.Beneficiary2();
      account.AddBeneficiary(beneficiary1, _clock.UtcNow);
      account.AddBeneficiary(beneficiary2,_clock.UtcNow);

      // Act
      account.RemoveBeneficiary(beneficiary1.Id,_clock.UtcNow);
    
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary1.Id);
      Null(actual);
      // Equal(beneficiary, actual);
   }
   #endregion
   
}