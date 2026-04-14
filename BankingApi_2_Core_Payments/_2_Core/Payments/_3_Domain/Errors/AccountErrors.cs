using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._3_Domain.Errors;

public static class AccountErrors {
   
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Id",
         Message: "The given Id is invalid.");
   
   public static readonly DomainErrors InvalidCustomerId =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid CustomerId",
         Message: "The given CustomerId is not valid.");
   
   public static readonly DomainErrors InvalidIbanFormat =
      new(ErrorCode.UnprocessableEntity,
         Title: "Account: Invalid Iban Format",
         Message: "The provided IBAN is not valid");
   
   public static readonly DomainErrors InvalidEmployeeId =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid EmployeeId",
         Message: "The EmployeeId is not valid.");

   public static readonly DomainErrors InvalidBalance =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Balance",
         Message: "The initial account balance must be zero or positive.");
   
   public static readonly DomainErrors InvalidCreditAmount =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Credit Amount",
         Message: "The credit amount must be greater than zero.");

   public static readonly DomainErrors InvalidDebitAmount =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Debit Amount",
         Message: "The debit amount must be greater than zero.");

   public static readonly DomainErrors InsufficientFunds =
      new(ErrorCode.Conflict,
         Title: "Account: Insufficient Funds",
         Message: "The account does not have sufficient funds for this operation.");

   public static readonly DomainErrors CurrencyMismatch =
      new(ErrorCode.BadRequest,
         Title: "Account: Currency Mismatch",
         Message: "The currency of the transaction does not match the account currency.");

   public static readonly DomainErrors InactiveAccount =
      new(ErrorCode.BadRequest, 
         Title: "Account: Is inactive",
         Message: "The given account is inactive.");

   public static readonly DomainErrors CustomerIdNotFoundOrInactive =
      new(ErrorCode.BadRequest, 
         Title: "Account: CustomerId Not Found or InActive",
         Message: "The given CustomerId not found or the Customer is inactive.");

   

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Account: Not found",
         Message: "No account with the given id exists."
      );
   
   public static readonly DomainErrors CustomerNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Account: Customer Not found",
         Message: "No customer with the given id exists."
      );
   
   public static readonly DomainErrors CustomerAlreadyHasAccount =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Account: Customer Already Has An Account",
         Message: "Initial account already exists for this owner."
      );
   
   public static readonly DomainErrors AuditRequiresEmployee =
      new(ErrorCode.BadRequest,
         Title: "Account: Employee required",
         Message: "This operation requires a valid employee id for auditing.");

   
}
