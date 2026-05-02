using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

internal sealed class CustomerUcUpdate(
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcUpdate> logger
)  {
   
   public async Task<Result<CustomerDto>> ExecuteAsync(
      Guid customerId,
      CustomerUpdateDto customerUpdateDto,
      CancellationToken ct = default
   ) {
     if(customerId == Guid.Empty)
        return Result<CustomerDto>.Failure(CustomerErrors.InvalidId);

      // 1) Find existing customer
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null) {
         logger.LogWarning("Update failed: customer found ({Id})", customerId.To8());
         return Result<CustomerDto>.Failure(CustomerErrors.NotFound);
      }
      if(!customer.IsActive)
         return Result<CustomerDto>.Failure(CustomerErrors.AlreadyDeactivated);
      
      // 2) DomainModel
      // check Email
      EmailVo? newEmailVo;
      if (customerUpdateDto.Email is null) {
         newEmailVo = null;
      }
      else {
         var resultEmail = EmailVo.Create(customerUpdateDto.Email);
         if (resultEmail.IsFailure) return Result<CustomerDto>.Failure(resultEmail.Error);
         newEmailVo = resultEmail.Value;
      }
      
      // check Address
      AddressVo? newAddressVo;
      if (customerUpdateDto.AddressDto is null) {
         newAddressVo = null;
      }
      else {
         var resultAddressVo = AddressVo.Create(
            street: customerUpdateDto.AddressDto!.Street,
            postalCode: customerUpdateDto.AddressDto!.PostalCode,
            city: customerUpdateDto.AddressDto!.City,
            country: customerUpdateDto.AddressDto!.Country
         );
         if (resultAddressVo.IsFailure) return Result<CustomerDto>.Failure(resultAddressVo.Error);
         newAddressVo = resultAddressVo.Value;
      }
      
      // update existing customer 
      var resultUpdate = customer.Update(
         lastname: customerUpdateDto.Lastname,
         companyName: customerUpdateDto.CompanyName,
         emailVo: newEmailVo, 
         addressVo: newAddressVo,
         updatedAt: clock.UtcNow
      );
      if (resultUpdate.IsFailure) 
         return Result<CustomerDto>.Failure(resultUpdate.Error);
      var updatedCustomer = resultUpdate.Value;

      // 3) Save changes to database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Update customer",ct);
      logger.LogDebug("Customer updated ({Id}, saved row {rows})", 
         customer.Id.To8(), savedRows);
      
      return Result<CustomerDto>.Success(updatedCustomer.ToCustomerDto());
   }

}