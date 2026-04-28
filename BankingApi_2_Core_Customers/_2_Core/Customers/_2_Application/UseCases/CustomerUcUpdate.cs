using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

public sealed class CustomerUcUpdate(
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcUpdate> logger
)  {
   
   public async Task<Result> ExecuteAsync(
      Guid customerId,
      CustomerUpdateDto customerUpdateDto,
      CancellationToken ct = default
   ) {
      // Find existing customer
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null) {
         logger.LogWarning("Update failed: customer found ({Id})", customerId.To8());
         return Result.Failure(CustomerErrors.NotFound);
      }

      // Check Email
      EmailVo? newEmailVo;
      if (customerUpdateDto.Email is null) {
         newEmailVo = null;
      }
      else {
         var resultEmail = EmailVo.Create(customerUpdateDto.Email);
         if (resultEmail.IsFailure) return Result.Failure(resultEmail.Error);
         newEmailVo = resultEmail.Value;
      }
      
      // Check Address
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
         if (resultAddressVo.IsFailure) return Result.Failure(resultAddressVo.Error);
         newAddressVo = resultAddressVo.Value;
      }
      
      // Update existing customer 
      var resultUpdate = customer.Update(
         lastname: customerUpdateDto.Lastname,
         companyName: customerUpdateDto.CompanyName,
         emailVo: newEmailVo, 
         addressVo: newAddressVo,
         updatedAt: clock.UtcNow
      );
      
      if (resultUpdate.IsFailure) 
         return Result.Failure(resultUpdate.Error);

      // Save changes in database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Email changes",ct);
      logger.LogDebug("Customer updated ({Id}, saved row {rows})", customer.Id.To8(), savedRows);
      
      return Result.Success();
   }

}