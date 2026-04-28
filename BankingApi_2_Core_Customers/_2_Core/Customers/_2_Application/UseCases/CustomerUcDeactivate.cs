using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

/// <summary>
/// Employee use case: deactivate a customer relationship
/// </summary>
public sealed class CustomerUcDeactivate(
   IEmployeeContract employeeContract,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      // 1) Validate input
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);

      // 2) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
         AdminRights.ManageAccounts, ct);   
      if(resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      // 4) Domain model
      var result = customer.Deactivate(
         deactivatedByEmployeeId: employeeContractDto.Id, 
         deactivatedAt: clock.UtcNow
      );
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var rows = await unitOfWork.SaveAllChangesAsync("Customer deactivated by employee", ct);
      logger.LogInformation("Account deactivated: {customerId} rows={rows}", customerId, rows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
