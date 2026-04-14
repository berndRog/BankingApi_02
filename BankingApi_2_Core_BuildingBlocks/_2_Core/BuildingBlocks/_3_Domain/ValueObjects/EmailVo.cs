using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;

// Email value object.
// Canonical stored representation.
//    Max.Mustermann@Example.COM   " -> "max.mustermann@example.com"

[ComplexType]
public sealed record EmailVo {
   
   //--- Properties ------------------------------------------------------------
   public string Value { get; private init; } 
   
   //--- Constructors ----------------------------------------------------------
   // EF Core ctor 
   private EmailVo() => Value = default!;
   
   // Domain ctor
   private EmailVo(string value) => Value = value;
   
   //--- Static Factories ------------------------------------------------------
   // Creates an Email from user input.
   public static Result<EmailVo> Create(string? input) {
      var resultNormalized = NormalizeFromInput(input);
      if (resultNormalized.IsFailure)
         return Result<EmailVo>.Failure(resultNormalized.Error);
      
      return Result<EmailVo>.Success(new EmailVo(resultNormalized.Value!));
   }

   //--- Factory - database (trusted) -----------------------------------------
   // Email from database value. Throws if DB contains corrupted data.
   public static EmailVo FromPersisted(string value) {
      if (!IsCanonical(value))
         throw new InvalidOperationException($"Invalid Email in database: '{value}'");
      return new EmailVo(value);
   }

   // Normalizes user input into canonical email form.
   // Steps:  1) Trim,  2) Lowercase,  3) Syntax validation
   private static Result<string> NormalizeFromInput(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      var email = input.Trim().ToLowerInvariant();

      // RFC max length
      if (email.Length > 254)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // simple structural sanity
      int at = email.IndexOf('@');
      if (at <= 0 || at >= email.Length - 1) 
         return Result<string>.Failure(CommonErrors.InvalidEmail);;
      if (email.Contains(' ')) 
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // split into both parts
      var token = email.Split('@');
      token[1] = token[1].Trim();
      // check whether right part has an endig with .xyz
      var rightToken = token[1].Split('.').ToList();
      if(rightToken.Count < 2)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      
      // Pragmatic syntax validation
      // (robust enough for real-world usage)
      try {
         _ = new MailAddress(email);
      }
      catch {
         return Result<string>.Failure(CommonErrors.InvalidEmail);
      }

      return Result<string>.Success(email);
   }


   // Cheap check ensuring DB value already follows canonical rules.
   // No normalization here — database must already be clean.
   private static bool IsCanonical(string value) {
      if (string.IsNullOrWhiteSpace(value)) return false;

      // must already be trimmed
      if (value != value.Trim()) return false;

      // must already be lowercase
      if (value != value.ToLowerInvariant()) return false;

      if (value.Length > 254) return false;

      // simple structural sanity
      var at = value.IndexOf('@');
      if (at <= 0 || at >= value.Length - 1) return false;

      if (value.Contains(' ')) return false;

      return true;
   }

   // Returns canonical email string.
   // </summary>
   public override string ToString() => Value;
}