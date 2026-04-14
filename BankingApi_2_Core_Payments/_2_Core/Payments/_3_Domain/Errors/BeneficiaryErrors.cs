using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._3_Domain.Errors;

public static class BeneficiaryErrors {

   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Beneficiary: Invalid Id",
         Message: "The provided beneficiary id is Invalid.");
   
   public static readonly DomainErrors IbanAlreadyRegistred =
      new(ErrorCode.Conflict, 
         Title: "Beneficiary with this IBAN Already Exists",
         Message: "The beneficiary is already registered for this account.");
   
   public static readonly DomainErrors InvalidName =
      new(ErrorCode.BadRequest,
         Title: "Beneficiary: Invalid Name",
         Message: "A name must not be provided.");


   public static readonly DomainErrors InvalidIban =
      new(ErrorCode.BadRequest, 
         Title: "Invalid IBAN",
         Message: "The provided IBAN is invalid.");
   
   public static readonly DomainErrors NotFound =
      new(ErrorCode.BadRequest, 
         Title: "Beneficiary not found",
         Message: "The beneficiary with the given id is not found.");
   
   
   public static readonly DomainErrors AccountNotFound =
      new(ErrorCode.BadRequest, 
         Title: "Beneficiary not found",
         Message: "The beneficiary with the given AccountId is not found.");
   
   public static readonly DomainErrors InValidAccountId =
      new(
         ErrorCode.BadRequest,
         Title: "Beneficiary: Invalid AccountId",
         Message: "The given accountId is invalid."
      );
   
   
}

