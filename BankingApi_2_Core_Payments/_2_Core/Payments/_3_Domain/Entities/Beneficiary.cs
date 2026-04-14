using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

// Child entity of Account Aggregate
public sealed class Beneficiary : Entity {
   
   //--- Properties ------------------------------------------------------------
   // inherited from Entity base class
   // public Guid Id { get; private set; } 
   public string Name { get; private set; } = string.Empty;
   public IbanVo IbanVo { get; private set; } = default!;
   
   // Account <-> Bneficiary [1] : [0..n]
   public Guid AccountId { get; private set; }

   //--- Constructors -----------------------------------------------------------
   // EfCore ctor
   private Beneficiary() {
   }

   // Domain ctor
   private Beneficiary(
      Guid id,
      string name,
      IbanVo ibanVo,
      Guid accountId
   ) {
      Id = id;
      AccountId = accountId;
      Name = name;
      IbanVo = ibanVo;
   }

   //--- Static Factory Methods ------------------------------------------------
   // static factory method to create a beneficiary
   public static Result<Beneficiary> Create(
      Guid accountId,
      string name,
      IbanVo ibanVo,
      string? id = null
   ) {
      // trim early
      name = name.Trim();

      if (string.IsNullOrWhiteSpace(name))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidName);
      
      var idResult = Resolve(id, BeneficiaryErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Beneficiary>.Failure(idResult.Error);
      var beneficiaryId = idResult.Value;

      var beneficiary = new Beneficiary(
         beneficiaryId, 
         name, 
         ibanVo, 
         accountId
      );

      return Result<Beneficiary>.Success(beneficiary);
   }
}