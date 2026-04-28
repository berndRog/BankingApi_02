using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Customers._3_Domain.Errors;

public static class CustomerErrors {
   
   // Identity & state
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Customer: Invalid Customer Id",
         Message: "The given Id is invalid.");

   public static readonly DomainErrors NotPending =
      new(
         ErrorCode.Conflict,
         Title: "Customer: Not in pending state",
         Message: "Only employees in pending state can be approved or rejected."
      );

   public static readonly DomainErrors AlreadyDeactivated =
      new(ErrorCode.Conflict,
         Title: "Customer:  Already deactivated",
         Message: "The owner has already been deactivated.");

   // Validation
   public static readonly DomainErrors FirstnameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: First name required",
         Message: "A first name must be provided.");

   public static readonly DomainErrors InvalidFirstname =
      new(ErrorCode.BadRequest,
         Title: "Customer: Invalid first name",
         Message: "The provided first name is too short or too long (2–80 characters).");

   public static readonly DomainErrors LastnameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: Last name required",
         Message: "A last name must be provided.");

   public static readonly DomainErrors InvalidLastname =
      new(ErrorCode.BadRequest,
         Title: "Customer: Invalid last name",
         Message: "The provided last name is too short or too long (2–80 characters).");
   
   public static readonly DomainErrors CompanyNameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: Company name required",
         Message: "A Company name must be provided.");

   public static readonly DomainErrors InvalidCompanyName =
      new(ErrorCode.BadRequest,
         Title: "Customer: Invalid company name",
         Message: "The provided company name is too short or too long (2–80 characters).");

   public static readonly DomainErrors AddressIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: Address required",
         Message: "A valid address with street, postal code and city must be provided.");

   public static readonly DomainErrors EmailIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: Email required",
         Message: "An email address must be provided.");
   
   public static readonly DomainErrors EmailNotFound =
      new(ErrorCode.NotFound,
         Title: "Customer: Not found by Email",
         Message: "No owner with the given email address exists.");
   
      
   public static readonly DomainErrors CreatedAtIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Customer: Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided.");
   // public static readonly DomainErrors UpdatedAtIsRequiredInUpdateAddress =
   //    new(ErrorCode.BadRequest,
   //       Title: "Customer: Update Timestamp Required when Updating Address",
   //       Message: "The update timestamp (updatedAt) must be provided.");
   // public static readonly DomainErrors UpdatedAtIsRequiredInRemoveAddress =
   //    new(ErrorCode.BadRequest,
   //       Title: "Customer: Update Timestamp Required when Removing Address",
   //       Message: "The update timestamp (updatedAt) must be provided.");
   // public static readonly DomainErrors UpdatedAtIsRequiredInUpdateEmail =
   //    new(ErrorCode.BadRequest,
   //       Title: "Customer: Update Timestamp Required when Updating Email",
   //       Message: "The update timestamp (updatedAt) must be provided.");

   
   public static readonly DomainErrors NotFound =
      new(ErrorCode.NotFound,
         Title: "Customer: Not found",
         Message: "No owner with the given id exists.");
   
   // Activation / rejection
   public static readonly DomainErrors AuditRequiresEmployee =
      new(ErrorCode.BadRequest,
         Title: "Customer: Employee required",
         Message: "This operation requires a valid employee id for auditing.");

   public static readonly DomainErrors ProfileIncomplete =
      new(ErrorCode.Conflict,
         Title: "Customer: Profile incomplete",
         Message: "The owner profile is incomplete. Complete the required profile data before activation.");

   public static readonly DomainErrors RejectionRequiresReason =
      new(ErrorCode.BadRequest,
         Title: "Customer: Rejection reason required",
         Message: "A rejection reason code must be provided.");
   
   public static readonly DomainErrors NotProvisioned =
      new(ErrorCode.NotFound,
         Title: "Customer: Is not provisioned",
         Message: "No customer with the given sub exists.");

   public static readonly DomainErrors EmployeesCannotUpdateCustomerProfile =
      new(ErrorCode.Conflict,
         Title: "Customer: Employee cannot update Customer profiles",
         Message: "The customer profile is blocked against employees access.");

   
   public static readonly DomainErrors EmployeeRightsRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Customer: Employee rights required",
         Message: "This operation requires employee privileges."
      );
   
   public static readonly DomainErrors EmailAlreadyInUse =
      new(ErrorCode.Conflict,
         Title: "Customer: Email Already Used",
         Message: "The customer email is already in use by another owner.");
   
   public static readonly DomainErrors EmailMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Customer: Email Must Be Unique",
         Message: "A customer with the given email address already exists."
      );
   
   public static readonly DomainErrors AlreadyHasAccounts =
      new(
         ErrorCode.Conflict,
         Title: "Customer: Already Has Accounts",
         Message: "A new customer must not have existing accounts."
      );
   
   public static readonly DomainErrors FilterIsRequired =
      new(ErrorCode.Conflict,
         Title: "Customer: Filter Is Required",
         Message: "The provided filter must not be null");

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Customer: Invalid Status Transitions",
         Message: "This operation is not possible, due to an invalid status transition."
      );


}

