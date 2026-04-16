using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal sealed class AccountUcBeneficiaryRemove(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcBeneficiaryRemove> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) {
      // 1) Find account with beneficiaries
      var account = 
         await accountRepository.FindAccountByIdWithBeneficiariesAsync(accountId, ct);
      if (account is null) 
         return Result.Failure(BeneficiaryErrors.AccountNotFound);
      
      // 2) Domain Model
      // Remove beneficiary from account
      account.RemoveBeneficiary(beneficiaryId, clock.UtcNow);
      
      // 3) Unit of work, save changes to database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Remove beneficiary", ct);

      logger.LogInformation("Beneficiary removed {id}, saedRow {rows})", 
         beneficiaryId.To8(), savedRows);
      
      return Result.Success();
   }
}