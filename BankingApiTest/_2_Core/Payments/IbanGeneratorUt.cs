using BankingApi._2_Core.Payments;
using BankingApiTest._2_Core;
namespace BankingApiTest.TestController._2_Core.Core.Payments;

public sealed class IbanGeneratorUt {

   [Fact]
   public void IbanBuildUt() {
      // Act
      var actual = IbanGenerator.Build();
      var display = IbanCheck.ToString(actual);
      // Assert
      NotNull(actual);
      var result = IbanCheck.Run(actual);
      True(result.IsSuccess);
   }
   
   [Fact]
   public void IbanBuildFromBbanUt() {
      // Act
      var actual = IbanGenerator.Build("DE", "2528 3442 2413 9386 23");
      var display = IbanCheck.ToString(actual);
      // Assert   DE78 2528 3442 2413 9386 23
      NotNull(actual);
      var result = IbanCheck.Run(actual);
      True(result.IsSuccess);
      Equal("DE78252834422413938623", actual);
      Equal("DE78 2528 3442 2413 9386 23", display);
      
   }
   
   [Fact]
   public void IbanFillTemplateUt() {
      // Act
      var actual = IbanGenerator.FillTemplateStart("DEXX 2528 3442 2413 9386 23");
      var display = IbanCheck.ToString(actual);
      // Assert   DE78 2528 3442 2413 9386 23
      NotNull(actual);
      var result = IbanCheck.Run(actual);
      True(result.IsSuccess);
      Equal("DE78252834422413938623", actual);
      Equal("DE78 2528 3442 2413 9386 23", display);
      
   }
   
   // [Fact]
   // public void IbanFillTemplate2Ut() {
   //    // Act
   //    var actual = IbanGenerator.FillTemplateEnd("70","10000000","00000000");
   //    var display = IbanCheck.ToString(actual);
   //    // Assert   DE78 2528 3442 2413 9386 23
   //    NotNull(actual);
   //    var result = IbanCheck.Run(actual);
   //    True(result.IsSuccess);
   //    Equal("DE78252834422413938623", actual);
   //    Equal("DE78 2528 3442 2413 9386 23", display);
   //    
   // }
   
}