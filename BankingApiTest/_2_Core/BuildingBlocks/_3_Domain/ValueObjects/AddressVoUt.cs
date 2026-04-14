using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Customers.Domain.Entities;

public sealed class AddressVoUt {
   private readonly TestSeed _seed = default!;
   private readonly AddressVo _addressVo = default!;

   public AddressVoUt() {
      _seed = new TestSeed();
      _addressVo = _seed.Address1Vo;
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" }; // too short (1)
      yield return new object[] { new string('A', 81) }; // too long (81)
   }
   
   [Fact]
   public void EqualsUt() {
      // Arrange
      var addressVo1 = AddressVo.Create("Herbert-Meyer-Str.7", "29556", "Sudernburg","DE").Value;
      var addressVo2 = AddressVo.Create("Herbert-Meyer-Str.7", "29556", "Sudernburg","DE").Value;
      
      // Act & Assert
      True(addressVo1.Equals(addressVo1));


   }
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void Invalid_street_fails(string street) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: street,
         postalCode: _addressVo.PostalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(street))
         Equivalent(CommonErrors.StreetIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidStreet, ResultAddress.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("A")]
   [InlineData("AAAAAAAAAAA")]
   public void Invalid_postal_code_fails(string postalCode) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: _addressVo.Street,
         postalCode: postalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(postalCode))
         Equivalent(CommonErrors.PostalCodeIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidPostalCode, ResultAddress.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void Invalid_city_fails(string city) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: _addressVo.Street,
         postalCode: _addressVo.PostalCode,
         city: city,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(city))
         Equivalent(CommonErrors.CityIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidCity, ResultAddress.Error);
   }



}