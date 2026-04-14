using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
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
      CustomerDto customerDto,
      CancellationToken ct = default
   ) {
      // Find existing customer
      var customer = await repository.FindByIdAsync(customerDto.Id, ct);
      if (customer is null) {
         logger.LogWarning("UpdateEmail email failed: owner not found ({Id})", customerDto.Id.To8());
         return Result.Failure(CustomerErrors.NotFound);
      }
      
      // Update existing customer 
      var resultUpdate = customer.Update(
         lastname: customer.Lastname,
         companyName: customer.CompanyName,
         emailVo: customer.EmailVo, 
         addressVo: customer.AddressVo,
         updatedAt: clock.UtcNow
      );
      
      if (resultUpdate.IsFailure) 
         return Result.Failure(resultUpdate.Error);

      // Save changes in database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Email changes",ct);
      logger.LogDebug("Customer updated ({Id}, saved row {rows})", customerDto.Id.To8(), savedRows);
      
      return Result.Success();
   }

}