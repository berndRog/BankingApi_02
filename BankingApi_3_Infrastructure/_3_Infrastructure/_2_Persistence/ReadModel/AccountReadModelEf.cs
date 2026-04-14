using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;

internal sealed class AccountReadModelEf(
   AppDbContext dbContext
) : IAccountReadModel {
   
   #region --- Aggregate root: Account ------------------------------------------------------
   public async Task<Result<AccountDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) {
      // the DB is doing the work: filter by Id, project to DTO, no tracking (read-only)
      var accountDto = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.Id == id) // filter
         .Select(c => c.ToAccountDto()) // projection
         .SingleOrDefaultAsync(ct);

      return accountDto is null
         ? Result<AccountDto>.Failure(AccountErrors.NotFound)
         : Result<AccountDto>.Success(accountDto);
   }

   public async Task<Result<AccountDto>> FindByIbanAsync(
      string iban,
      CancellationToken ct
   ) {
      var result = IbanVo.Create(iban);
      if (result.IsFailure)
         throw new ApplicationException(result.Error.Message);
      var ibanVo = result.Value;

      var accountDto = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.IbanVo == ibanVo) // filter
         .Select(c => c.ToAccountDto()) // projection
         .SingleOrDefaultAsync(ct); // take single or default (null if not found)

      return accountDto is null
         ? Result<AccountDto>.Failure(AccountErrors.NotFound)
         : Result<AccountDto>.Success(accountDto);
   }

   public async Task<Result<IEnumerable<AccountDto>>> SelectAsync(
      CancellationToken ctToken = default
   ) {
      var accountDtos = await dbContext.Accounts
         .AsNoTracking()
         .Select(a => a.ToAccountDto())
         .ToListAsync(ctToken);
      return Result<IEnumerable<AccountDto>>.Success(accountDtos);
   }

   public async Task<Result<IEnumerable<AccountDto>>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   ) {
      // 1. Basic validation for the GUID
      if (customerId == Guid.Empty)
         return Result<IEnumerable<AccountDto>>.Failure(AccountErrors.InvalidCustomerId);

      // 2. Consistent with the Beneficiary logic: 
      // We check if the "Parent" (Customer) exists to avoid returning a 
      // "false empty" list if the ID is simply wrong.
      var accountDtos = await dbContext.Accounts
         .Where(a => a.CustomerId == customerId)
         .Select(a => a.ToAccountDto())
         .ToListAsync(ct);
      
      // 3. Case: Customer not found in the database
      if (accountDtos.Count == 0)
         return Result<IEnumerable<AccountDto>>.Failure(AccountErrors.CustomerNotFound);

      // 4. Case: Customer exists, returning their accounts (can be an empty list [])
      return Result<IEnumerable<AccountDto>>.Success(accountDtos);
   }
   #endregion
   
   #region --- Child Entities: Beneficiaries ------------------------------------------------
   public async Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) {
      // 1. Fetch the specific beneficiary ensuring it belongs to the provided AccountId.
      // We use AsNoTracking for read-only performance and project directly to a DTO.
      var beneficiaryDto = await dbContext.Beneficiaries
         .AsNoTracking()
         .Where(b => b.AccountId == accountId && b.Id == beneficiaryId)
         .Select(b => b.ToBeneficiaryDto())
         .SingleOrDefaultAsync(ct);

      // 2. If no record matches both IDs, we return a NotFound failure.
      // This covers both cases: either the AccountId is wrong or the BeneficiaryId is wrong.
      return beneficiaryDto is null ? 
         Result<BeneficiaryDto>.Failure(BeneficiaryErrors.NotFound) :
         Result<BeneficiaryDto>.Success(beneficiaryDto);
   }

   public async Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) {
      // 1. Query the database using the Aggregate Root (Account) as the entry point.
      // We use a projection (Select) to check for account existence and 
      // retrieve the list of beneficiaries in a single database roundtrip.
      var result = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.Id == accountId)
         .Select(a => new {
            // Project the children directly to DTOs
            Beneficiaries = a.Beneficiaries.Select(b => b.ToBeneficiaryDto()).ToList()
         })
         .SingleOrDefaultAsync(ct);

      // 2. Case: The AccountId does not exist in the database.
      // We return a failure because the requested context (the Account) is invalid.
      if (result == null) {
         return Result<IEnumerable<BeneficiaryDto>>
            .Failure(BeneficiaryErrors.InValidAccountId);
      }

      // 3. Case: The Account exists, but may have zero beneficiaries.
      // We return Success with an empty list [] because an account with 
      // no beneficiaries is a valid state, not an error.
      return Result<IEnumerable<BeneficiaryDto>>.Success(result.Beneficiaries);
   }

   public async Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      Guid accountId,
      string name,
      CancellationToken ct = default
   ) {
      // 1. Sanitize the search input
      var searchName = name.Trim();

      // 2. Query starting from the Aggregate Root (Account) to ensure context validity.
      // We use a projection to fetch existence and filtered data in one DB trip.
      var result = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.Id == accountId)
         .Select(a => new {
            // Filter children directly within the projection
            FilteredBeneficiaries = a.Beneficiaries
               .Where(b => b.Name.Contains(searchName))
               .Select(b => b.ToBeneficiaryDto())
               .ToList()
         })
         .SingleOrDefaultAsync(ct);

      // 3. Case: Account does not exist (Invalid ID provided)
      if (result == null) {
         return Result<IEnumerable<BeneficiaryDto>>
            .Failure(BeneficiaryErrors.InValidAccountId);
      }

      // 4. Case: Account exists, but search may yield an empty list []
      return Result<IEnumerable<BeneficiaryDto>>.Success(result.FilteredBeneficiaries);
   }
   #endregion
   
}
