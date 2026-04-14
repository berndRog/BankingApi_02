using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApiTest._2_Core.Customers.Domain.Entities;

public sealed class EmailVoUt {

   
   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" }; // too short (1)
      yield return new object[] { new string('A', 300)+"@xyz.com" }; 
   }
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("micky@mouse")]
   [InlineData("@")]
   [InlineData("@.com")]
   [InlineData("abc.yzx@.com")]
   [InlineData("a.b.de")]
   public void Invalid_email_fails(string email) {
      // Act
      var result = EmailVo.Create(email);
      // Assert
      False(result.IsSuccess);
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }
}