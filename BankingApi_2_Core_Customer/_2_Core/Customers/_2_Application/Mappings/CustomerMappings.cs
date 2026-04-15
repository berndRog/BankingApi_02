using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
namespace BankingApi._2_Core.Customers._2_Application.Mappings;

public static class CustomerMappings {

   public static CustomerCreateDto ToCustomerCreateDto(this Customer customer) => new(
      Id:          customer.Id,
      Firstname:   customer.Firstname,
      Lastname:    customer.Lastname,
      CompanyName: customer.CompanyName,
      StatusInt:  (int) customer.Status,
      Email:       customer.EmailVo.Value,
      Subject:     customer.Subject,
      AddressDto:  customer.AddressVo.ToAddressDto(),
      AccountId: null,
      Iban: null,
      Balance: null
   );
   
   
   public static CustomerDto ToCustomerDto(this Customer customer) => new(
      Id:          customer.Id,
      Firstname:   customer.Firstname,
      Lastname:    customer.Lastname,
      CompanyName: customer.CompanyName,
      StatusInt: (int) customer.Status,
      Email: customer.EmailVo.Value,
      AddressDto: customer.AddressVo.ToAddressDto()
   );
   
   public static CustomerProvisionDto ToCustomerProvisionDto(this Customer customer, bool wasCreated) => new(
      Id: customer.Id,
      WasCreated: wasCreated
   );

   public static CustomerDetailsDto ToCustomerDetailsDto(this Customer customer) => new(
      Id: customer.Id,
      Firstname: customer.Firstname,
      Lastname: customer.Lastname,
      CompanyName: customer.CompanyName,
      StatusInt: (int)customer.Status,
      ActivatedAt: customer.ActivatedAt?.ToString("O"),
      RejectedAt: customer.RejectedAt?.ToString("O"),
      RejectCodeInt: (int)customer.CustomerRejectCode,
      AuditedByEmployeeId: customer.AuditedByEmployeeId,
      DeactivatedAt: customer.DeactivatedAt?.ToString("O"),
      DeactivatedByEmployeeId: customer.DeactivatedByEmployeeId,
      Email: customer.EmailVo.Value,
      AddressDto: customer.AddressVo.ToAddressDto()
   );
   


}
