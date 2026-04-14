using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApiTest._3_Infrastructure._5_Utils;
namespace BankingApiTest.TestInfrastructure;

public sealed class TestSeed {
   private DateTimeOffset _utcNow;
   private IClock _clock;
   private Seed _seed;
   
   public IClock Clock => _clock;

   public TestSeed() {
      _utcNow = DateTimeOffset.Parse("2025-01-01T00:00:00Z");
      _clock = new FakeClock(_utcNow);
      _seed = new Seed(_clock);
   }
   
   public string Employee2Id => _seed.Employee2Id;
   
   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1Vo => _seed.Address1Vo;
   public AddressVo Address2Vo => _seed.Address2Vo;
   public AddressVo Address3Vo => _seed.Address3Vo;
   public AddressVo Address4Vo => _seed.Address4Vo;
   public AddressVo Address5Vo => _seed.Address5Vo;
   #endregion

   #region -------------- Test Customers (Enities) -------------------------------------------
   public Customer Customer1() => _seed.Customer1();
   public Customer Customer2() => _seed.Customer2();
   public Customer Customer3() => _seed.Customer3();
   public Customer Customer4() => _seed.Customer4();
   public Customer Customer5() => _seed.Customer5();
   public Customer Customer6() => _seed.Customer6();
   
   public Customer CustomerRegister() => _seed.CustomerRegister();

   public IReadOnlyList<Customer> Customers => [
      Customer1(), Customer2(), Customer3(), Customer4(), Customer5(), Customer6()
   ];
   #endregion

   #region -------------- Test Iban (Value Objects) ------------------------------------------
   public string Iban1 => Seed.Iban1;
   public string Iban2 => Seed.Iban2;
   public string Iban3 => Seed.Iban3;
   public string Iban4 => Seed.Iban4;
   public string Iban5 => Seed.Iban5;
   public string Iban6 => Seed.Iban6;
   public string Iban7 => Seed.Iban7;
   public string Iban8 => Seed.Iban8;
   #endregion
   
   #region -------------- Test Accounts ------------------------------------------------------
   public Account Account1() => _seed.Account1();
   public Account Account2() => _seed.Account2();
   public Account Account3() => _seed.Account3();
   public Account Account4() => _seed.Account4();
   public Account Account5() => _seed.Account5();
   public Account Account6() => _seed.Account6();
   public Account Account7() => _seed.Account7();
   public Account Account8() => _seed.Account8();

   public IReadOnlyList<Account> Accounts => new List<Account>() {
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8()
   };
   #endregion

   #region -------------- Test Beneficiaries -------------------------------------------------
   public Beneficiary Beneficiary1() => _seed.Beneficiary1();
   public Beneficiary Beneficiary2() => _seed.Beneficiary2();
   public Beneficiary Beneficiary3() => _seed.Beneficiary3();
   public Beneficiary Beneficiary4() => _seed.Beneficiary4();
   public Beneficiary Beneficiary5() => _seed.Beneficiary5();
   public Beneficiary Beneficiary6() => _seed.Beneficiary6();
   public Beneficiary Beneficiary7() => _seed.Beneficiary7();
   public Beneficiary Beneficiary8() => _seed.Beneficiary8();
   public Beneficiary Beneficiary9() => _seed.Beneficiary9();
   public Beneficiary Beneficiary10() => _seed.Beneficiary10();
   public Beneficiary Beneficiary11() => _seed.Beneficiary11();
   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>() {
      Beneficiary1(), Beneficiary2(), Beneficiary3(), Beneficiary4(),
      Beneficiary5(), Beneficiary6(), Beneficiary7(), Beneficiary8(),
      Beneficiary9(), Beneficiary10(), Beneficiary11()
   };
   #endregion

   public List<Account> AddBeneficiariesToAccounts() 
      =>  _seed.AddBeneficiariesToAccounts();

}