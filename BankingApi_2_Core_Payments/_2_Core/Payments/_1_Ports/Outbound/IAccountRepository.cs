using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;

namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Repository port for accessing Account aggregates.
// Used by application use cases to load and persist accounts and their beneficiaries.
// Implemented in the Infrastructure layer (e.g. EF Core).
public interface IAccountRepository {

   //---- Aggregate root: Account ----------------------------------------------    
   // Load an account aggregate by identifier
   Task<Account?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Load an account using an IBAN value object
   Task<Account?> FindByIbanAsync(
      IbanVo ibanVo,
      CancellationToken ct = default
   );
   
   // Check whether a customer already owns an account
   Task<bool> ExistsByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // Load all accounts for a customer
   Task<IReadOnlyList<Account>> SelelctByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   
   // Add a new account aggregate to the persistence context
   void Add(Account account);
   void AddRange(IEnumerable<Account> accounts);
   
   //---- Child Entity: Beneficiary --------------------------------------------      
   // Loads the Account Aggregate Root and all its Beneficiary child entities.
   Task<Account?> FindAccountByIdWithBeneficiariesAsync(
      Guid accountId,
      CancellationToken ct = default
   );
   
   // Loads the Account root and attach the specific Beneficiary we want to modify.ficiary
   Task<Account?> FindAccountByWithBeneficiaryByIdAsync (
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );

   // Add Beneficiary as added in the tracker
   void Add(Beneficiary beneficiary);
   void AddRange(IEnumerable<Beneficiary> beneficiaries);

   // Remove a beneficiary from the persistence context
   void Remove(Beneficiary beneficiary);
   
   
}
