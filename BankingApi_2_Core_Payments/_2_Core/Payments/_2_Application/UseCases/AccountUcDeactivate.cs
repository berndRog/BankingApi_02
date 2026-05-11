using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal sealed class AccountUcDeactivate(
   IEmployeeContract employeeContract,
   IAccountRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      Guid accountId,
      CancellationToken ct
   ) {
      if (accountId == Guid.Empty)
         return Result.Failure(AccountErrors.InvalidCustomerId);
      if (accountId == Guid.Empty)
         return Result.Failure(AccountErrors.InvalidId);
      
      // 1) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
          AdminRights.ManageAccounts, ct);   
      if(resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 2) Load account from database
      var account = await repository.FindByIdAsync(accountId, ct);
      if (account is null)
         return Result.Failure(AccountErrors.NotFound);
      if (customerId != account.CustomerId)
         return Result.Failure(AccountErrors.ConflictCustomerId);

      // 3) Domain model
      var deactivatedAt = clock.UtcNow;
      var employeeId = employeeContractDto.Id;
      var result = account.Deactivate(employeeId, deactivatedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error);
      
      // 4)  Unit of work, save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Account deactivated by employee", ct);
      logger.LogInformation("Customer deactivated customerId={customerId} rows={rows}", accountId, rows);

      return Result.Success();
   }
}