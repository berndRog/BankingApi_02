using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._3_Domain.ValueObjects;


// IBAN value object (DE/AT/CH).
//
// Canonical persisted form:
// - no spaces/separators
// - uppercase
// Validation:
// - allowed countries + exact length (D/A/CH)
// - characters A-Z / 0-9 only
// - MOD-97 checksum must pass
[ComplexType]
public sealed record class IbanVo {
   
   //--- Properties ------------------------------------------------------------
   // Canonical IBAN (uppercase, no separators).
   // Example: "de44 5001 0517 5407 3249 31" -> "DE44500105175407324931"
   public string Value { get; private init; }
 
   //--- Constructors ----------------------------------------------------------
   // EF Core ctor  
   private IbanVo() => Value = default!;
   
   // Domain ctor
   private IbanVo(string normalized) => Value = normalized;
   
   //--- Static Factories ------------------------------------------------------
   // Creates an Iban from user input.
   public static Result<IbanVo> Create(string? input) {
      var normalized = NormalizeFromInput(input);

      if (!TryValidate(normalized, out var error))
         return Result<IbanVo>.Failure(InvalidIban(error));

      return Result<IbanVo>.Success(new IbanVo(normalized));
   }

   // Recreate Iban from database value.
   public static IbanVo FromPersisted(string value) {
      if (!IsCanonical(value))
         throw new InvalidOperationException($"Invalid Iban in database: '{value}'");

      // Full checksum validation is still cheap enough and gives safety.
      // If you want "ultra-cheap", remove the MOD-97 call here.
      if (!TryValidate(value, out var error))
         throw new InvalidOperationException($"Invalid Iban in database: '{value}'. {error}");

      return new IbanVo(value);
   }

   // Human readable IBAN (groups of 4).
   // Example: "DE44500105175407324931" -> "DE44 5001 0517 5407 3249 31"
   public static string ToString(string iban) => Format(iban);

   
   // NORMALIZE user input into canonical form:
   // - removes whitespace and common separators (- . _)
   // - uppercases letters
   private static string NormalizeFromInput(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return string.Empty;

      var sb = new StringBuilder(input.Length);
      foreach (var ch in input) {
         if (char.IsWhiteSpace(ch) || ch is '-' or '.' or '_')
            continue;

         sb.Append(char.ToUpperInvariant(ch));
      }

      return sb.ToString();
   }
   
   // Cheap check ensuring DB value already follows canonical rules:
   private static bool IsCanonical(string value) {
      if (string.IsNullOrWhiteSpace(value))
         return false;

      // must already be trimmed (no leading/trailing whitespace)
      if (value != value.Trim())
         return false;

      for (int i = 0; i < value.Length; i++) {
         char c = value[i];

         // reject typical separators explicitly
         if (char.IsWhiteSpace(c) || c is '-' or '.' or '_')
            return false;

         // must be uppercase A-Z or digit
         if (!(char.IsDigit(c) || IsUpperAlpha(c)))
            return false;
      }

      return true;
   }

   // Human readable IBAN (groups of 4).
   // Example: "DE44500105175407324931" -> "DE44 5001 0517 5407 3249 31"
   public override string ToString() => Format(Value);
   
   // VALIDATION (structure + checksum)
   private static bool TryValidate(string normalized, out string error) {
      if (normalized.Length == 0)
         return Fail(out error, "IBAN is required.");

      if (normalized.Length < 4)
         return Fail(out error, "IBAN is too short.");

      var country = normalized.Substring(0, 2);

      if (!AllowedCountries.TryGetValue(country, out var expectedLen))
         return Fail(out error,
            $"IBAN country '{country}' is not allowed (expected one of: {string.Join(", ", AllowedCountries.Keys)}).");

      if (normalized.Length != expectedLen)
         return Fail(out error,
            $"IBAN length for '{country}' must be {expectedLen} characters (was {normalized.Length}).");

      // check digits must be numeric
      if (!char.IsDigit(normalized[2]) || !char.IsDigit(normalized[3]))
         return Fail(out error, "IBAN check digits (positions 3-4) must be numeric.");

      // allowed chars
      for (int i = 0; i < normalized.Length; i++) {
         var c = normalized[i];
         if (!(char.IsDigit(c) || IsUpperAlpha(c)))
            return Fail(out error, $"IBAN contains invalid character '{c}'. Only A-Z and 0-9 are allowed.");
      }

      // MOD-97 checksum
      if (!PassesMod97(normalized))
         return Fail(out error, "IBAN checksum (MOD-97) is invalid.");

      error = string.Empty;
      return true;
   }

   private static bool Fail(out string error, string message) {
      error = message;
      return false;
   }

   // FORMAT canonical iban into groups of 4 characters for display.
   private static string Format(string iban) {
      if (string.IsNullOrEmpty(iban))
         return string.Empty;

      var sb = new StringBuilder(iban.Length + iban.Length / 4);

      for (int i = 0; i < iban.Length; i++) {
         if (i > 0 && i % 4 == 0) sb.Append(' ');
         sb.Append(iban[i]);
      }

      return sb.ToString();
   }

   // CHECKSUM (MOD-97)
   private static bool PassesMod97(string iban) {
      // Move first four chars to the end
      var rearranged = iban.Substring(4) + iban.Substring(0, 4);

      var mod = 0;
      for (var i = 0; i < rearranged.Length; i++) {
         char c = rearranged[i];

         if (char.IsDigit(c)) {
            mod = (mod * 10 + (c - '0')) % 97;
            continue;
         }

         if (IsUpperAlpha(c)) {
            var val = (c - 'A') + 10; // A=10 ... Z=35
            mod = (mod * 10 + (val / 10)) % 97;
            mod = (mod * 10 + (val % 10)) % 97;
            continue;
         }

         return false; // should never happen due to earlier validation
      }

      return mod == 1;
   }

   private static bool IsUpperAlpha(char c) => c >= 'A' && c <= 'Z';
   
   // COUNTRY RULES (DACH)
   private static readonly IReadOnlyDictionary<string, int> AllowedCountries =
      new Dictionary<string, int>(StringComparer.Ordinal) {
         ["DE"] = 22,
         ["AT"] = 20,
         ["CH"] = 21,
      };
   
   private static DomainErrors InvalidIban(string message) =>
      new(ErrorCode.BadRequest, Title: "Account: Invalid Iban", Message: message);

}