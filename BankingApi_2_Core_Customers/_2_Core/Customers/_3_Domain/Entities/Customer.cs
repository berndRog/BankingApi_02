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
   // public DateTime CreatedAt { get; private set; }
   // public DateTime UpdatedAt { get; private set; }
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public string? CompanyName { get; private set; }

   // Display name used in UIs and documents (derived, not persisted)
   public string DisplayName => CompanyName ?? $"{Firstname} {Lastname}";
   
   // Subject identifier from the identity provider (OIDC / OAuth)
   public string Subject { get; private set; } = default!;
   
   // Value Objects EmailVo
   public EmailVo EmailVo { get; private set; } = default!;

   // Status (business lifecycle)
   public CustomerStatus Status { get; private set; } = CustomerStatus.Pending;

   // Value Object AddressVo
   public AddressVo AddressVo { get; private set; } = default!; 
   
   // Employee decisions (audit facts)
   public DateTime? ActivatedAt { get; private set; }
   public DateTime? RejectedAt { get; private set; }
   public CustomerRejectCode RejectCode { get; private set; }
   public Guid? AuditedByEmployeeId { get; private set; }

   public DateTime? DeactivatedAt { get; private set; }
   public Guid? DeactivatedByEmployeeId { get; private set; }

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

   //--- constructors ----------------------------------------------------------
   // EF Core ctor
   private Customer() {
   }

   // Domain ctor (used by factories)
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
      Subject = subject;
      EmailVo = emailVo;
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
      DateTime createdAt = default!,
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
      
      var resultSubject = IdentitySubject.Check(subject);
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
   
   //--- domain methods --------------------------------------------------------
   // Employee activates the customer after external identity verification.
   // Activation is only possible if the customer is Pending and profile is complete.
   public Result Activate(
      Guid auditeddByEmployeeId,
      DateTime activatedAt
   ) {
      if (activatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      // fail early if preconditions for activation are not met
      // (employee, timestamp, status, profile)
      if (auditeddByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.NotPending);
      
      if (!IsProfileComplete)
         return Result.Failure(CustomerErrors.ProfileIncomplete);

      Status = CustomerStatus.Active;
      ActivatedAt = activatedAt;
      AuditedByEmployeeId = auditeddByEmployeeId;

      RejectedAt = null;
      RejectCode = CustomerRejectCode.None;

      Touch(activatedAt);
      return Result.Success();
   }
   
   // Employee deactivates the customer (end customer relationship).
   public Result Deactivate(
      Guid deactivatedByEmployeeId,
      DateTime deactivatedAt
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
   public Result<Customer> Update(
      string? lastname = null,
      string? companyName = null,
      EmailVo? emailVo = null,
      AddressVo? addressVo = null,
      DateTime updatedAt = default
   ) {
      if (updatedAt == default)
         return Result<Customer>.Failure(CommonErrors.TimestampIsRequired);
      if (addressVo is null)
         return Result<Customer>.Failure(CustomerErrors.AddressIsRequired);

      lastname = lastname?.Trim();
      companyName = companyName?.Trim();

      if (!string.IsNullOrWhiteSpace(lastname) && lastname.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidCompanyName);

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
      return Result<Customer>.Success(this);
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