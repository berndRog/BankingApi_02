using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
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
      // 2) Load authorized employee and check if has rights to manage accounts
      // var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
      //    AdminRights.ManageAccounts, ct);   
      // if(resultEmployee.IsFailure)
      //   return Result<AccountDto>.Failure(resultEmployee.Error);
      // var employeeContractDto = resultEmployee.Value;
      var employeeContractDto = new EmployeeContractDto(
         Id: Guid.Parse("00000000-0002-0000-0000-000000000000"),
         AdminRightsInt: 511 // all AdminRights
      );
      
      // 2) Validate input
      if (accountId == Guid.Empty)
          return Result.Failure(AccountErrors.InvalidId);

      // 3) Load account from database
      var account = await repository.FindByIdAsync(accountId, ct);
      if (account is null)
         return Result.Failure(AccountErrors.NotFound);

      // 4) Domain model
      var deactivatedAt = clock.UtcNow;
      var employeeId = employeeContractDto.Id;
      var result = account.Deactivate(employeeId, deactivatedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error)
            .LogIfFailure(logger, "AccountUcDeactivate", new { accountId, employeeId, deactivatedAt });

      // 5)  Unit of work, save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Account deactivated by employee", ct);
      logger.LogInformation("Customer deactivated customerId={customerId} rows={rows}", accountId, rows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
