using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class AccountMappings {

   public static AccountDto ToAccountDto(this Account account) => new(
      Id: account.Id,
      Iban: account.IbanVo.Value,
      Balance: account.BalanceVo.Amount,
      Currency: (int) account.BalanceVo.Currency,
      CustomerId: account.CustomerId   
   );

   public static BeneficiaryDto ToBeneficiaryDto(this Beneficiary beneficiary) => new(
      Id: beneficiary.Id,
      Name: beneficiary.Name,
      Iban: beneficiary.IbanVo.Value,
      AccountId: beneficiary.AccountId
   );
   
   public static AccountContractDto ToAccountContractDto(this Account account) => new(
      Id: account.Id,
      Iban: account.IbanVo.Value,
      CustomerId: account.CustomerId   
   );
   
}


