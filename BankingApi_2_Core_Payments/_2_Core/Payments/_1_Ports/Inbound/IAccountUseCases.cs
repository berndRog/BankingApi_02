using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;

namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

// Application port defining command use cases for the Accounts domain.
// Used by API controllers to trigger state changes in the account aggregate.
// Represents the write side of the Payments / Accounts context.
public interface IAccountUseCases {

   // Create a new account for a customer
   Task<Result<AccountDto>> CreateAsync(
      Guid customerId,
      AccountDto accountDto,
      CancellationToken ct = default
   );
   
   // deactivate an account 
   Task<Result> DeactivateAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // Add a beneficiary to an account
   Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   );

   // Remove a beneficiary from an account
   Task<Result> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );

}