using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

internal sealed class CustomerUcDeactivate(
   IAccountContract accountContract,
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
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);

      // 1) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
         AdminRights.ManageAccounts, ct);   
      if(resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 2) Domain model
      // deactivate all accounts and delete according beneficiaries
      var resultDeactivateAccounts = await accountContract.DeactivateAllAccountsAsync(
         customerId: customerId, 
         ct: ct
      );
      if(resultDeactivateAccounts.IsFailure)
         return Result.Failure(resultDeactivateAccounts.Error);
      
      // deactivate customer
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      var result = customer.Deactivate(
         deactivatedByEmployeeId: employeeContractDto.Id, 
         deactivatedAt: clock.UtcNow
      );
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 3) Save all changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Customer deactivated", ct);
      logger.LogInformation("Account deactivated: {customerId} rows={rows}", customerId, rows);

      return Result.Success();
   }
}
