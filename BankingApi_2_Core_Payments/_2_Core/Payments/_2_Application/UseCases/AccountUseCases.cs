using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal class AccountUseCases(
   AccountUcCreate accountUcCreate,
   AccountUcDeactivate accountUcDeactivate,
   AccountUcBeneficiaryAdd accountUcBeneficiaryAdd,
   AccountUcBeneficiaryRemove accountUcBeneficiaryRemove
) : IAccountUseCases {
   
   public Task<Result<AccountDto>> CreateAsync(
      Guid customerId,
      AccountDto accountDto,
      CancellationToken ct = default
   ) => accountUcCreate.ExecuteAsync(customerId, accountDto, ct);
   
   public Task<Result> DeactivateAsync(
      Guid accountId,
      CancellationToken ct = default
   ) => accountUcDeactivate.ExecuteAsync(accountId, ct);
   
   public Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   ) => accountUcBeneficiaryAdd.ExecuteAsync(accountId, beneficiaryDto, ct);
   
   public Task<Result> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) => accountUcBeneficiaryRemove.ExecuteAsync(accountId, beneficiaryId, ct);
   
}