using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Read model interface for querying accounts, beneficiaies and transsfer data.
public interface IAccountReadModel {

   #region accounts
   // Find account by technical identifier
   Task<Result<AccountDto>> FindByIdAsync(
      Guid id,
      CancellationToken ctToken = default
   );

   // Find account using an IBAN
   Task<Result<AccountDto>> FindByIbanAsync(
      string iban,
      CancellationToken ct
   );

   // Return all accounts
   Task<Result<IEnumerable<AccountDto>>> SelectAsync(
      CancellationToken ctToken = default
   );

   // Return all accounts owned by a specific customer
   Task<Result<IEnumerable<AccountDto>>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   #endregion
   
   #region beneficiaries
   // Find a beneficiary by identifier
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );

   // Return all beneficiaries of an account
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // Search beneficiaries by name
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      Guid accountId,
      string name,
      CancellationToken ct = default
   );
   #endregion

   // Optional filtering / paging query
   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );
}
