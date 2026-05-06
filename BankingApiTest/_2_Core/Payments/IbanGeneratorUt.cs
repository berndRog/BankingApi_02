using BankingApi._2_Core.Payments._3_Domain;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApiTest.TestController._2_Core.Core.Payments;

public sealed class IbanGeneratorUt {

   [Fact]
   public void IbanBuildUt() {
      // Act
      var actual = IbanGenerator.Build();
      var display = IbanVo.ToString(actual);
      // Assert
      NotNull(actual);
      var result = IbanVo.Create(actual);
      True(result.IsSuccess);
   }
   
   [Fact]
   public void IbanBuildFromBbanUt() {
      // Act
      var actual = IbanGenerator.Build("DE", "2528 3442 2413 9386 23");
      var display = IbanVo.ToString(actual);
      // Assert   DE78 2528 3442 2413 9386 23
      NotNull(actual);
      var result = IbanVo.Create(actual);
      True(result.IsSuccess);
      Equal("DE78252834422413938623", actual);
      Equal("DE78 2528 3442 2413 9386 23", display);
      
   }
   
   [Fact]
   public void IbanFillTemplateStartUt() {
      // Act
      var actual = IbanGenerator.FillTemplateStart("DEXX 2528 3442 2413 9386 23");
      var display = IbanVo.ToString(actual);
      // Assert   DE78 2528 3442 2413 9386 23
      NotNull(actual);
      var result = IbanVo.Create(actual);
      True(result.IsSuccess);
      Equal("DE78252834422413938623", actual);
      Equal("DE78 2528 3442 2413 9386 23", display);
      
   }
   
   [Fact]
   public void IbanFillTemplateEndUt() {
      // Act
      var actual = IbanGenerator.FillTemplateEnd("76","10000000","00001234");
      var display = IbanVo.ToString(actual);
      // Assert   DE76 1000 0000 0000 1234 02
      NotNull(actual);
      var result = IbanVo.Create(actual);
      True(result.IsSuccess);
      Equal("DE76100000000000123402", actual);
      Equal("DE76 1000 0000 0000 1234 02", display);
      
   }
   
}