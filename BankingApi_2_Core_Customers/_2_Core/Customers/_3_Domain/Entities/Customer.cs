using System.Diagnostics.CodeAnalysis;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
namespace BankingApi._2_Core.Customers._3_Domain.Entities;

public sealed class Customer : AggregateRoot {
   //--- properties ------------------------------------------------------------
   // inherited from Entity + Aggregate root base class
   // public Guid Id { get; private set; } 
   // public DateTimeOffset CreatedAt { get; private set; }
   // public DateTimeOffset UpdatedAt { get; private set; }
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public string? CompanyName { get; private set; }
     
   // Display name used in UIs and documents (derived, not persisted)
   public string DisplayName => CompanyName ?? $"{Firstname} {Lastname}";

   // Value Objects EmailVo
   public EmailVo EmailVo { get; private set; } = default!;

   // Status (business lifecycle)
   public CustomerStatus Status { get; private set; } = CustomerStatus.Pending;

   // Value Object AddressVo
   public AddressVo AddressVo { get; private set; } = default!; 
   
   // Employee decisions (audit facts)
   public DateTimeOffset? ActivatedAt { get; private set; }
   public DateTimeOffset? RejectedAt { get; private set; }
   public CustomerRejectCode CustomerRejectCode { get; private set; }
   public Guid? AuditedByEmployeeId { get; private set; }

   public DateTimeOffset? DeactivatedAt { get; private set; }
   public Guid? DeactivatedByEmployeeId { get; private set; }
   
   // Subject identifier from the identity provider (OIDC / OAuth)
   public string Subject { get; private set; } = default!;
   
   // Derived state (read convenience, not persisted)
   public bool IsProfileComplete =>
      !string.IsNullOrWhiteSpace(Firstname) &&
      !string.IsNullOrWhiteSpace(Lastname) &&
      !string.IsNullOrWhiteSpace(Subject) &&
      !string.IsNullOrWhiteSpace(EmailVo.Value) &&
      !string.IsNullOrWhiteSpace(AddressVo.Street) &&
      !string.IsNullOrWhiteSpace(AddressVo.PostalCode) &&
      !string.IsNullOrWhiteSpace(AddressVo.City);

   public bool IsActive =>
      Status == CustomerStatus.Active &&
      DeactivatedAt is null;

   //--- ctors -----------------------------------------------------------------
   // EF Core constructor
   private Customer() {
   }

   // Domain constructor (used by factories)
   [SetsRequiredMembers]
   private Customer(
      Guid id,
      string firstname,
      string lastname,
      string? companyName,
      string subject,
      EmailVo emailVo,
      AddressVo addressVo
   ) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      CompanyName = companyName;
      EmailVo = emailVo;
      Subject = subject;
      AddressVo = addressVo;
   }

   // --- static factory to create a Customer object ---------------------------
   // Create a Customer with an account and activate it
   public static Result<Customer> Create(
      string firstname,
      string lastname,
      string? companyName,
      string subject,
      EmailVo emailVo,
      AddressVo addressVo,
      DateTimeOffset createdAt = default!,
      string? id = null
   ) {
      // Normalize inputs early
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      companyName = companyName?.Trim();

      // Validate basic fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Customer>.Failure(CustomerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Customer>.Failure(CustomerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidCompanyName);
      
      var resultSubject = SubjectCheck.Run(subject);
      if (resultSubject.IsFailure)
         return Result<Customer>.Failure(resultSubject.Error);

      // Resolve (or generate) aggregate id
      var resultId = Resolve(id, CustomerErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Customer>.Failure(resultId.Error);
      var customerId = resultId.Value;

      var customer = new Customer(
         id: customerId,
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         subject: resultSubject.Value,
         emailVo: emailVo,
         addressVo: addressVo
      );

      // set timestamps
      customer.Initialize(createdAt);

      return Result<Customer>.Success(customer);
   }

   // Create an owner on first login (provisioning).
   public static Result<Customer> CreateProvision(
      string subject,
      EmailVo emailVo,
      DateTimeOffset createdAt = default!,
      string? id = null
   ) {
      
      var resultId = Resolve(id, CustomerErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Customer>.Failure(resultId.Error);
      var customerId = resultId.Value;

      var addressVo = AddressVo.Create(
         street: "undefined",
         postalCode: "undefined",
         city: "undefined",
         country: null
      ).Value;
      
      // Provisioned customer starts with identity data plus required business profile data.
      var customer = new Customer(
         id: customerId,
         firstname: "undefined",
         lastname: "undefined",
         companyName: null,
         subject: subject,
         emailVo: emailVo,
         addressVo: addressVo
      );

      // Provisioning should reflect identity creation time in IA-Server
      customer.Initialize(createdAt);

      return Result<Customer>.Success(customer);
   }

   // --------------------------------------------------------------------------
   // Domain methods (mutations)
   // --------------------------------------------------------------------------
   // Customer completes or updates their profile after provisioning.
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string? companyName,
      EmailVo emailVo,
      AddressVo addressVo,
      DateTimeOffset updatedAt
   ) {
      if (updatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      firstname = firstname.Trim();
      lastname = lastname.Trim();
      companyName = companyName?.Trim();

      // Validate required profile fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(CustomerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(CustomerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidCompanyName);

      // Apply changes
      Firstname = firstname;
      Lastname = lastname;
      CompanyName = companyName;
      EmailVo = emailVo;
      AddressVo = addressVo;

      // SELF-SERVICE: if profile is complete, we auto-activate the owner without employee involvement.
      // auto-activate on profile completion (no employee involved)
      Activate(Guid.Empty, updatedAt);

      Touch(updatedAt);

      return Result.Success();
   }

   // Employee activates the owner after external identity verification.
   // Activation is only possible if the owner is Pending and profile is complete.
   public Result Activate(
      Guid activatedByEmployeeId,
      DateTimeOffset activatedAt
   ) {
      if (activatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      // fail early if preconditions for activation are not met
      // (employee, timestamp, status, profile)
      if (activatedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.NotPending);
      if (!IsProfileComplete)
         return Result.Failure(CustomerErrors.ProfileIncomplete);

      Status = CustomerStatus.Active;
      ActivatedAt = activatedAt;
      AuditedByEmployeeId = activatedByEmployeeId;

      RejectedAt = null;
      CustomerRejectCode = CustomerRejectCode.None;

      // create initial account for the owner (domain event, handled in application layer)
      Touch(activatedAt);
      return Result.Success();
   }
   
   // Employee rejects the owner (e.g., KYC failed).
   public Result Reject(
      Guid rejectedByEmployeeId,
      CustomerRejectCode customerRejectCode,
      DateTimeOffset rejectedAt
   ) {
      if (rejectedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      // fail early if preconditions for rejection are not met
      // (employee, timestamp, status, reason code)
      if (rejectedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (customerRejectCode == default)
         return Result.Failure(CustomerErrors.RejectionRequiresReason);
      if (Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.NotPending);

      Status = CustomerStatus.Rejected;
      RejectedAt = rejectedAt;
      AuditedByEmployeeId = rejectedByEmployeeId;
      CustomerRejectCode = customerRejectCode;

      Touch(rejectedAt);
      return Result.Success();
   }

   // Employee deactivates the customer (end customer relationship).
   public Result Deactivate(
      Guid deactivatedByEmployeeId,
      DateTimeOffset deactivatedAt
   ) {
      if (deactivatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      // fail early if preconditions for deactivation are not met
      // (employee, timestamp, status)
      if (deactivatedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (Status == CustomerStatus.Deactivated)
         return Result.Failure(CustomerErrors.AlreadyDeactivated);

      Status = CustomerStatus.Deactivated;
      DeactivatedAt = deactivatedAt;
      DeactivatedByEmployeeId = deactivatedByEmployeeId;

      Touch(deactivatedAt);
      return Result.Success();
   }
   
   // Customer updates their profile
   public Result Update(
      string? lastname,
      string? companyName,
      EmailVo? emailVo,
      AddressVo? addressVo,
      DateTimeOffset updatedAt
   ) {
      if (updatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      if (addressVo is null)
         return Result.Failure(CustomerErrors.AddressIsRequired);

      lastname = lastname?.Trim();
      companyName = companyName?.Trim();

      if (!string.IsNullOrWhiteSpace(lastname) && lastname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidCompanyName);

      // var resultEmail = EmailVo.Create(email);
      // if (resultEmail.IsFailure)
      //    return Result.Failure(resultEmail.Error);
      // var emailVo = resultEmail.Value;
      
      // Apply changes
      if (lastname is not null) Lastname = lastname;
      if (companyName is not null) CompanyName = companyName;
      if (emailVo is not null) EmailVo = emailVo;
      if (addressVo is not null) AddressVo = addressVo;

      Touch(updatedAt);
      return Result.Success();
   }
}

/*
=============================================================================
Didaktik & Lernziele (Vorlesung BankingAPI / DDD)
=============================================================================

1) Aggregate Root & Invarianten
- Customer ist Aggregate Root: Status-Übergänge (Pending/Active/Rejected/Deactivated)
  und fachliche Regeln (z.B. Aktivierung nur bei vollständigem Profil) liegen im
  Aggregate und sind dort zentral testbar.

2) Stammdaten vs. Prozesse (Onboarding)
- Provisioning (CreateProvision) ist ein technischer Startpunkt nach OIDC-Login.
  Danach folgt ein fachlicher Prozess:
  Profil vervollständigen -> Mitarbeiterprüfung (extern, KYC/AML) -> Activate/Reject.

3) Status als Fachkonzept + Audit-Facts
- Status ist ein fachlicher Zustand.
- ActivatedAt/RejectedAt/AuditedByEmployeeId/ReasonCode sind Audit-Fakten:
  Sie unterstützen Nachvollziehbarkeit, Compliance und spätere Reports.

4) Value Objects (Address) im Domainmodell
- Address ist ein verpflichtendes Value Object.
- Street, PostalCode und City sind erforderlich; Country bleibt optional.
- Der UI-Transport kann flach sein, im Domainmodell bleibt die Fachstruktur klar.

5) Zeit und Testbarkeit (IClock / now Injection)
- Domain-Methoden bekommen 'now' als Parameter, um deterministische Tests zu
  ermöglichen und nicht von einem internen Clock-Zustand nach EF-Laden abhängig
  zu sein. CreatedAt wird beim Provisioning bewusst auf identity-createdAt gesetzt.

6) Architektur-Überleitung (BC-Schnitt)
- Customer-BC besitzt Customer-Datenhoheit.
- Kontoanlage bei Aktivierung passiert NICHT im Customer-Aggregate, sondern als
  Orchestrierung im Application UseCase (Customer aktivieren + initial Account anlegen).
  Damit bleibt die BC-Grenze sauber (Customer-BC ≠ Accounts-BC).

=============================================================================
*/