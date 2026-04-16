using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal sealed class AccountUcBeneficiaryAdd(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcBeneficiaryAdd> logger
) {
   
   public async Task<Result<BeneficiaryDto>> ExecuteAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   ) {
      // 1) Load account with beneficiaries
      var account = await accountRepository.FindAccountByIdWithBeneficiariesAsync(accountId, ct);
      if (account is null) 
         return Result<BeneficiaryDto>.Failure(BeneficiaryErrors.AccountNotFound);
      
      // 2) Domain Model
      // create IbanVo
      var resultIban = IbanVo.Create(beneficiaryDto.Iban);
      if (resultIban.IsFailure) 
         return Result<BeneficiaryDto>.Failure(BeneficiaryErrors.InvalidIban);
      var ibanVo = resultIban.Value;
      
      // Create a new beneficiary
      var resultBeneficiary = Beneficiary.Create(
         accountId: accountId,
         name: beneficiaryDto.Name,
         ibanVo: ibanVo,
         id: beneficiaryDto.Id.ToString()
      );
      if (resultBeneficiary.IsFailure)
         return Result<BeneficiaryDto>.Failure(resultBeneficiary.Error);
      
      // Add beneficiary to account
      var result = account.AddBeneficiary(
         beneficiary:   resultBeneficiary.Value,
         updatedAt: clock.UtcNow
      );
      if (result.IsFailure) 
         return Result<BeneficiaryDto>.Failure(result.Error);
      var beneficiary = result.Value;
      
      // 3) Unit of work, save changes to database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Add beneficiary to account", ct);

      logger.LogDebug("Beneficiary added ({Id}) to Account ({AccountId}) savedRows: {Rows}",
         beneficiary.Id.To8(), accountId.To8(), savedRows);

      return Result<BeneficiaryDto>.Success(beneficiary.ToBeneficiaryDto());
   }
}
