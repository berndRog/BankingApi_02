using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
namespace BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;

/// <summary>
/// Common domain errors shared across bounded contexts.
/// These errors are not specific to a single aggregate.
/// </summary>
public static class CommonErrors {

   // Identity / communication
   // ------------------------------------------------------------------------
   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Email: Invalid email address",
         Message: "The provided email address is not valid."
      );

   public static readonly DomainErrors InvalidPhone =
      new(
         ErrorCode.BadRequest,
         Title: "Phone: Invalid phone number",
         Message: "The provided phone number is not valid."
      );

   public static readonly DomainErrors InvalidMoneyAmount =
      new(ErrorCode.BadRequest,
         Title: "Money: Invalid Amount",
         Message: "The monetary amount must be greater than or equal to zero and have at most two decimal places.");

   
   // Address (Value Object)
   // ------------------------------------------------------------------------
   public static readonly DomainErrors StreetIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Address: Street is required",
         Message: "A street must be provided when specifying an address."
      );
   public static readonly DomainErrors InvalidStreet =
      new(ErrorCode.BadRequest,
         Title: "Address: Invalid street name",
         Message: "The provided street name is too short or too long (2–80 characters).");
   
   public static readonly DomainErrors PostalCodeIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Address: Postal code is required",
         Message: "A postal code must be provided when specifying an address."
      );
   public static readonly DomainErrors InvalidPostalCode =
      new(ErrorCode.BadRequest,
         Title: "Address: Invalid postal code",
         Message: "The provided postal code is too short or too long (2–10 characters).");
   
   
   public static readonly DomainErrors CityIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Address:City is required",
         Message: "A city must be provided when specifying an address."
      );
   public static readonly DomainErrors InvalidCity =
      new(ErrorCode.BadRequest,
         Title: "Address: Invalid city",
         Message: "The provided city is too short or too long (2–80 characters).");
   
   public static readonly DomainErrors InvalidCountry =
      new(ErrorCode.BadRequest,
         Title: "Address: Invalid country",
         Message: "The provided country is too short or too long (2–80 characters).");

   // Time / auditing
   // ------------------------------------------------------------------------
   public static readonly DomainErrors TimestampIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Timestamp is required",
         Message: "A valid timestamp must be provided for this operation."
      );

   // Identity / authentication context
   // ------------------------------------------------------------------------
   public static readonly DomainErrors IdentityClaimsMissing =
      new(
         ErrorCode.Unauthorized,
         Title: "Identity claims missing",
         Message: "Required identity information is missing from the authentication context."
      );   
   

   public static readonly DomainErrors InvalidIdentitySubject =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid IdentitySubject",
         Message: "The provided sub is not valid."
      );
   
   public static readonly DomainErrors Forbidden =
      new(
         ErrorCode.Forbidden,
         Title: "Access denied",
         Message: "You are authenticated but not allowed to perform this action."
      );
   
   
   public static readonly DomainErrors IbanNotValid =
      new(
         ErrorCode.BadRequest,
         Title: "Given IBAN is not valid",
         Message: "The given IBAN is not a valid IBAN number."
      );

}