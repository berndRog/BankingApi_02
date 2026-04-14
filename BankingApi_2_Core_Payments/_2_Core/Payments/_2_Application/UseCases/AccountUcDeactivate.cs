using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal sealed class AccountUcDeactivate(
   IIdentityGateway identityGateway,
   IAccountRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid accountId,
      CancellationToken ct
   ) {
      // // 1) Authorization: must be an employee/admin with the required rights
      // if (identityGateway.AdminRights == 0)
      //    return Result.Failure(CustomerErrors.EmployeeRightsRequired);
      //
      // // 2) Validate input
      // if (accountId == Guid.Empty)
      //    return Result.Failure(CustomerErrors.InvalidId);

      // 3) Load aggregate
      var account = await repository.FindByIdAsync(accountId, ct);
      if (account is null)
         return Result.Failure(AccountErrors.NotFound);

      // 4) Domain mutation
      var deactivatedAt = clock.UtcNow;
      var employeeId = ParseEmployeeId(identityGateway.Subject);
      var result = account.Deactivate(employeeId, deactivatedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error)
            .LogIfFailure(logger, "AccountUcDeactivate", new { accountId, employeeId, deactivatedAt });

      // 5) Persist
      var rows = await unitOfWork.SaveAllChangesAsync("Account deactivated by employee", ct);
      logger.LogInformation("Customer deactivated customerId={customerId} rows={rows}", accountId, rows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
