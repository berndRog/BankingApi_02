using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

internal sealed class AccountUcCreate(
   ICustomerContract customerContract,
   // IEmployeeContract employeeContract,
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcCreate> logger
) {
   
   public async Task<Result<AccountDto>> ExecuteAsync(
      Guid customerId,
      AccountDto accountDto,
      CancellationToken ct = default
   ) {
      // 1) Exits Customer with given id and is active?
      var resultCustomer = await customerContract.ExistsActiveCustomerAsync(customerId, ct);
      if (resultCustomer.IsFailure)
         return Result<AccountDto>.Failure(AccountErrors.CustomerIdNotFoundOrInactive);
      
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
      
      // 3) domain model  
      var resultIbanVo = IbanVo.Create(accountDto.Iban);
      if (resultIbanVo.IsFailure)
         return Result<AccountDto>.Failure(resultIbanVo.Error);
      var ibanVo = resultIbanVo.Value;
      
      var resultBalanceVo = MoneyVo.Create(accountDto.Balance, (Currency) accountDto.Currency);
      if (resultBalanceVo.IsFailure)
         return Result<AccountDto>.Failure(resultBalanceVo.Error);
      var balanceVo = resultBalanceVo.Value;
      
      // create entity
      var result = Account.Create(
         ibanVo: ibanVo, 
         balanceVo: balanceVo, 
         customerId: customerId,
         createdByEmployeeId: employeeContractDto.Id,
         createdAt: clock.UtcNow,
         id: accountDto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<AccountDto>.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcCreate.DomainRejected",
               new { accountDto });
      var account = result.Value;
      
      // 4) add to repository
      accountRepository.Add(account);            
         
      // 5) unit of work, save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Add account", ct);
      
      logger.LogInformation("AccountUcCreate={id} rows={rows}", account.Id, rows);
      
      return Result<AccountDto>.Success(account.ToAccountDto());
   }
}