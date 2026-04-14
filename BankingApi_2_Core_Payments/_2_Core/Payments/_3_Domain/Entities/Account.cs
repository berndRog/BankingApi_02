using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Account : AggregateRoot {
   
   //--- Properties ------------------------------------------------------------
   // inherited from Entity + Aggregate root base class
   // public Guid Id { get; private set; } 
   // public DateTimeOffset CreatedAt { get; private set; }
   // public DateTimeOffset UpdatedAt { get; private set; }

   // IBAN as a domain value object.
   public IbanVo IbanVo { get; private set; } = default!;
   
   // Account balance as a domain value object.
   public MoneyVo BalanceVo { get; private set; } = default!;

   // Employee decisions (audit facts)
   public Guid? CreatedByEmployeeId { get; private set; }
   public DateTimeOffset? DeactivatedAt { get; private set; } = null;
   public Guid? DeactivatedByEmployeeId { get; private set; }
   
   public bool IsActive => DeactivatedAt == null;

   // BC: Customer <-> Account [1] : [0..n]
   public Guid CustomerId { get; private set; }

   // Child Entities: Account <-> Beneficiary [1] : [0..n]
   private readonly List<Beneficiary> _beneficiaries = new();
   public IReadOnlyCollection<Beneficiary> Beneficiaries => 
      _beneficiaries.AsReadOnly();
   
   
   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Account() { }

   // Domain ctor
   private Account(
      Guid id,
      IbanVo ibanVo,
      MoneyVo balanceVo,
      Guid customerId,
      Guid createdByEmployeeId
   )  {
      Id = id;
      IbanVo = ibanVo;
      BalanceVo = balanceVo;
      CustomerId = customerId;
      CreatedByEmployeeId = createdByEmployeeId;
   }

   //--- Static Factory --------------------------------------------------------
   // Static factory method to create a new account for an existing cutomer.
   public static Result<Account> Create(
      IbanVo ibanVo,
      MoneyVo balanceVo,
      Guid customerId,
      Guid createdByEmployeeId,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      // invariant: customerId must be valid
      if (customerId == Guid.Empty)
         return Result<Account>.Failure(AccountErrors.InvalidCustomerId);
      
      if (createdByEmployeeId == Guid.Empty)
         return Result<Account>.Failure(AccountErrors.InvalidEmployeeId);
      
      var idResult = Resolve(id, AccountErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Account>.Failure(idResult.Error);
      var accountId = idResult.Value;

      if(balanceVo.Amount < 0)
         return Result<Account>.Failure(AccountErrors.InvalidBalance);
      
      // create entity
      var account = new Account(
         id: accountId, 
         customerId: customerId, 
         ibanVo: ibanVo, 
         balanceVo: balanceVo,
         createdByEmployeeId: createdByEmployeeId
      );
      
      // 
      account.Initialize(createdAt);
      
      return Result<Account>.Success(account);
   }

   //--- Domain operations -----------------------------------------------------
   // Employee deactivates the customer (end customer relationship).
   public Result Deactivate(
      Guid deactivatedByEmployeeId,
      DateTimeOffset deactivatedAt
   ) {
      if (deactivatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      if (deactivatedByEmployeeId == Guid.Empty)
         return Result.Failure(AccountErrors.AuditRequiresEmployee);
      
      DeactivatedAt = deactivatedAt;
      DeactivatedByEmployeeId = deactivatedByEmployeeId;

      Touch(deactivatedAt);
      return Result.Success();
   }
   
   
   #region -------------------- Beneficiaries ------------------------------------------
   // Story 3.1: add a beneficiary to THIS account
   public Result<Beneficiary> AddBeneficiary(
      Beneficiary beneficiary,
      DateTimeOffset updatedAt
   ) {
      // check for duplicate IBANs
      if (_beneficiaries.Any(b => b.IbanVo.Equals(beneficiary.IbanVo)))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.IbanAlreadyRegistred);
      
      // add to collection
      _beneficiaries.Add(beneficiary);
      Touch(updatedAt); 

      return Result<Beneficiary>.Success(beneficiary);
   }

   public Result<Beneficiary> FindBeneficiary(
      Guid beneficiaryId
   ) {
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      return found is null
         ? Result<Beneficiary>.Failure(BeneficiaryErrors.NotFound)
         : Result<Beneficiary>.Success(found);
   }

   public Result<Guid> RemoveBeneficiary(
      Guid beneficiaryId,
      DateTimeOffset updatedAt
   ) {
      if (beneficiaryId == Guid.Empty)
         return Result<Guid>.Failure(BeneficiaryErrors.InvalidId);

      // find beneficiary
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      if (found is null)
         return Result<Guid>.Failure(BeneficiaryErrors.NotFound);

      // remove from collection
      _beneficiaries.Remove(found);
      Touch(updatedAt); // update audit info

      return Result<Guid>.Success(beneficiaryId);
   }
   #endregion
   
}