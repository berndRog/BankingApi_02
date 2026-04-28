using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Customers.Domain.Entities;

public sealed class CustomerUt {
   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;
   private readonly Customer _customer;
   private readonly Customer _customer5; // with CompanyName
   private readonly AddressVo _addressVo = default!;
   private readonly Guid _employee2Id;

   public CustomerUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      _customer = _seed.Customer1();
      _customer5 = _seed.Customer5();
      _addressVo = _seed.Address1Vo;
      _employee2Id = Guid.Parse(_seed.Employee2Id);
      
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" }; // too short (1)
      yield return new object[] { new string('A', 81) }; // too long (81)
   }

   #region--- CreatePerson tests ------------------------------------------------------
   [Fact]
   public void CreateCustomer_valid_input_and_id_creates_customer() {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      Equal(_customer.Id, actual.Id);
      Equal(_customer.Firstname, actual.Firstname);
      Equal(_customer.Lastname, actual.Lastname);
      Equal(_customer.CompanyName, actual.CompanyName);
      Equal(_customer.DisplayName, actual.DisplayName);
      Equal(_customer.EmailVo, actual.EmailVo);
      Equal(_customer.Subject, actual.Subject);
      Equal(_customer.Status, actual.Status);
      Equal(_customer.AddressVo, actual.AddressVo);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Fact]
   public void CreateCustomer_valid_input_and_without_id() {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: null // <== without id
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      False(actual.Id == Guid.Empty);
      Equal(_customer.Firstname, actual.Firstname);
      Equal(_customer.Lastname, actual.Lastname);
      Equal(_customer.CompanyName, actual.CompanyName);
      Equal(_customer.Subject, actual.Subject);
      Equal(_customer.EmailVo, actual.EmailVo);
      Equal(_addressVo, actual.AddressVo);
      Equal(_customer.DisplayName, actual.DisplayName);
      Equal(_customer.Status, actual.Status);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_firstname_fails(string firstname) {
      // Act
      var result = Customer.Create(
         firstname: firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_invalid_firstname_length_fails(string firstname) {
      var result = Customer.Create(
         firstname: firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_lastname_fails(string lastname) {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_invalid_lastname_length_fails(string lastname) {
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidLastname, result.Error);
   }

   [Fact]
   public void CreateCustomer_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }
   #endregion

   #region --- CreateCompany tests -----------------------------------------------------
   [Fact]
   public void CreateCompany_ok() {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: _customer5.CompanyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      IsType<Customer>(actual);
      Equal(_customer5.Id, actual.Id);
      Equal(_customer5.Firstname, actual.Firstname);
      Equal(_customer5.Lastname, actual.Lastname);
      Equal(_customer5.CompanyName, actual.CompanyName);
      Equal(_customer5.DisplayName, actual.DisplayName);
      Equal(_customer5.EmailVo, actual.EmailVo);
      Equal(_customer5.Subject, actual.Subject);
      Equal(_customer5.Status, actual.Status);
      Equal(_customer5.AddressVo, actual.AddressVo);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_companyName_length_ok(string companyName) {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: companyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      True(result.IsSuccess);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCompany_invalid_companyName_length_fails(string companyName) {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: companyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidCompanyName, result.Error);
   }
   #endregion
}