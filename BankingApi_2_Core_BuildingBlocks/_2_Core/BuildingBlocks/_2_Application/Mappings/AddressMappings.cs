using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;

public static class AddressMappings {
   public static AddressDto ToAddressDto(this AddressVo address) => new(
      Street: address.Street,
      PostalCode: address.PostalCode,
      City: address.City,
      Country: address.Country
   );
   
   public static AddressVo ToAddressVo(this AddressDto addressDto) {
      
      var result = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );
      if(result.IsFailure)
         throw new InvalidOperationException($"Invalid address data: {result.Error}");
      return result.Value;
   }
}
