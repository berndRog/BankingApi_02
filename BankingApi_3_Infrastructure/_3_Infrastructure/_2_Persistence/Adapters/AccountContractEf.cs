using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using IbanGenerator = BankingApi._2_Core.BuildingBlocks.Utils.IbanGenerator;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal class AccountContractEf(
   IEmployeeContract employeeContract,
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountContractEf> logger
): IAccountContract{
   
   public async Task<Result<AccountContractDto>> OpenInitialAccountAsync(
      Guid customerId, 
      string? accoutIdString = null,
      string? iban = null,
      decimal? balance = null,
      int currency = 1, // EUR
      CancellationToken ct = default!
   ) {
      // 1) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
         AdminRights.ManageAccounts, ct);   
      if(resultEmployee.IsFailure)
         return Result<AccountContractDto>.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 2) Create IBAN (generate if not provided, validate if provided)
      if (string.IsNullOrEmpty(iban)) {
         // generate iban
         iban = IbanGenerator.CreateGermanIban(); 
      }
      else if (iban.Contains("DEXX")) {
         // validate iban format DEXX 1234 1234 1234 1234 00
         // and generate valid check digits XX
         try {
            iban = IbanGenerator.CreateGermanIban(iban);
         }
         catch (FormatException) {
            return Result<AccountContractDto>.Failure(AccountErrors.InvalidIbanFormat);
         }
      }
      var resultIbanVo = IbanVo.Create(iban);
      if(resultIbanVo.IsFailure)
         return Result<AccountContractDto>.Failure(resultIbanVo.Error);
      var ibanVo = resultIbanVo.Value;
      
      // 3) Create BalanceVo
      if(balance == null) balance = 0m;
      var resultBalanceVo = MoneyVo.Create((decimal) balance, (Currency) currency);
      if (resultBalanceVo.IsFailure)
         return Result<AccountContractDto>.Failure(resultBalanceVo.Error);
      var balanceVo = resultBalanceVo.Value;

      // 4) Create Aggregate root Account 
      var resultAccount = Account.Create(
         ibanVo: ibanVo,
         balanceVo: balanceVo,
         customerId: customerId,
         createdByEmployeeId: employeeContractDto.Id, 
         createdAt: clock.UtcNow,
         id: accoutIdString
      );
      if(resultAccount.IsFailure)
         return Result<AccountContractDto>.Failure(resultAccount.Error);
      var account = resultAccount.Value;
      
      // 5) Add to repository
      accountRepository.Add(account);
      
      // 6) Save all changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Initial account", ct);
      logger.LogInformation(
         "Initial account created customerId={ownId} accountId {accId} rows={rows}", 
         customerId, account.Id, rows);
      
      return Result<AccountContractDto>.Success(account.ToAccountContractDto());
   }

   public async Task<Result<bool>> HasNoAccountsAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      // Has Customer allready an account?
      var exits = await accountRepository.ExistsByCustomerIdAsync(customerId, ct);

      return !exits is true
         ? Result<bool>.Success(true)  // has no accounts
         : Result<bool>.Failure(AccountErrors.CustomerAlreadyHasAccount);
   }

   public async Task<Result> DeactivateAllAccountsAsync(
      Guid customerId,
      CancellationToken ct = default!
   ) {
      // 1) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
         AdminRights.ManageAccounts, ct);
      if (resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 2) Load all accounts with beneficiaries for a given customer
      var accounts = 
         await accountRepository.SelelctAccountsByCustomerIdWithBeneficiariesAsync(customerId, ct);
      
      // 3) Delete beneficiaries first and then the accounts 
      foreach(var account in accounts) {
         // delete all beneficiaries in account and database
         var snapShot = account.Beneficiaries.ToList(); // create copy to avoid modification during iteration
         foreach (var beneficiary in snapShot) {
            var resultBeneficiary = account.RemoveBeneficiary(beneficiary.Id, clock.UtcNow);
            if(resultBeneficiary.IsFailure)
               return Result.Failure(resultBeneficiary.Error);
            accountRepository.Remove(beneficiary); 
         }
         // deactivate account
         account.Deactivate(
            deactivatedByEmployeeId: employeeContractDto.Id, 
            deactivatedAt: clock.UtcNow
         );
      }
      
      // 4) Save all changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Deactivate account", ct);
      unitOfWork.ClearChangeTracker(); // clear tracked entities to avoid side effects in later operations in the same transaction
      
      logger.LogInformation(
         "Deactivate accounts for customerId={customerId} rows={rows}", 
         customerId, rows);
      
      return Result.Success();
   }
}