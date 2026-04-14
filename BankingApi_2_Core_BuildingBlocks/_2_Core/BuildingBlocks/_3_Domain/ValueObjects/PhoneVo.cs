using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;

// Phone number value object.
//
// Canonical persisted form:
// - digits only (E.164 normalized)
// - no spaces, separators or formatting characters
// - country code mandatory (e.g. 4915112345678)
//
// Presentation form:
// - human readable formatted version (e.g. +49 151 1234 5678)
//
// Design rule:
// - Create(...)        = user input (normalization + validation + country rules)
// - FromPersisted(...) = database value (cheap invariant check only)
//
// Motivation:
// The canonical form guarantees stable comparisons, uniqueness checks,
// indexing and reliable equality semantics independent from formatting.
[ComplexType]
public sealed record PhoneVo {

   //--- Properties ------------------------------------------------------------
   // Canonical stored representation.
   public string Value { get; private init; }

   //--- Constructors ----------------------------------------------------------
   // EF Core ctor
   private PhoneVo() => Value = default!;
   
   // Domain ctor
   private PhoneVo(string value) => Value = value;

   //--- Static Factories ------------------------------------------------------
   // User input
   public static Result<PhoneVo> Create(string? input) {
      var normalized = NormalizeFromInput(input);
      if (normalized.IsFailure)
         return Result<PhoneVo>.Failure(normalized.Error);
      return Result<PhoneVo>.Success(new PhoneVo(normalized.Value!));
   }

   // database 
   public static PhoneVo FromPersisted(string value) {
      if (!IsCanonical(value))
         throw new InvalidOperationException($"Invalid phone in database: '{value}'");
      return new PhoneVo(value);
   }

   // NORMALIZATION (only used during Create)
   // Input	                  Output
   // 0511 1234 567	         05111234567
   // +49 511 1234 567	      +495111234567
   // +49 0511 1234567	      +495111234567
   // 0049 0511 1234567	      +495111234567
   // 0049 (0)511 1234567	   +495111234567
   // +49 511/1234-567        +495111234567
   // +49 511 / 12 34 - 567   +495111234567
   private static Result<string> NormalizeFromInput(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      var number = input.Trim();

      if (!Allowed.IsMatch(number))
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      //--- STEP 1: 00 prefix -> +
      if (number.StartsWith("00"))
         number = "+" + number.Substring(2);

      bool international = number.StartsWith("+");

      // remove "(0)"
      number = OptionalTrunkZero.Replace(number, "");

      // keep digits (but remember +)
      var digits = Regex.Replace(number, @"\D", "");

      if (digits.Length < 7 || digits.Length > 15)
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      //--- STEP 2: remove trunk 0 after country code
      if (international)
         digits = RemoveTrunkZeroAfterCountryCode(digits);

      return Result<string>.Success(international ? "+" + digits : digits);
   }

   private static string RemoveTrunkZeroAfterCountryCode(string digits) {
      // Known DACH country codes
      ReadOnlySpan<string> dach = ["49", "41", "43"];

      foreach (var cc in dach) {
         if (digits.StartsWith(cc) && digits.Length > cc.Length && digits[cc.Length] == '0')
            return cc + digits.Substring(cc.Length + 1); // drop the trunk zero
      }

      return digits;
   }

   // Invariant Check for DB
   private static bool IsCanonical(string value) {
      if (string.IsNullOrWhiteSpace(value))
         return false;

      ReadOnlySpan<char> span = value.AsSpan();

      if (span[0] == '+')
         span = span[1..];

      if (span.Length < 7 || span.Length > 15)
         return false;

      foreach (var c in span)
         if (c < '0' || c > '9')
            return false;

      return true;
   }

   // DISPLAY BEHAVIOR
   public override string ToString()
      => Value.StartsWith('+')
         ? FormatInternational(Value)
         : FormatLocal(Value);

   private static string FormatInternational(string value) {
      var digits = value[1..];
      var ccLength = GuessCountryCodeLength(digits);

      var country = digits[..ccLength];
      var rest = digits[ccLength..];

      return "+" + country + " " + GroupFromRight(rest);
   }

   private static string FormatLocal(string digits)
      => GroupFromRight(digits);

   private static string GroupFromRight(string digits) {
      var sb = new StringBuilder();
      int first = digits.Length % 4;
      if (first == 0) first = 4;

      sb.Append(digits[..first]);

      for (int i = first; i < digits.Length; i += 4) {
         sb.Append(' ');
         sb.Append(digits.Substring(i, Math.Min(4, digits.Length - i)));
      }

      return sb.ToString();
   }

   private static int GuessCountryCodeLength(string digits) {
      if (digits.StartsWith("49") || digits.StartsWith("41") || digits.StartsWith("43"))
         return 2;

      return digits.Length > 10 ? 3 : 1;
   }

   // REGEX (only for input)
   private static readonly Regex Allowed =
      new(@"^(?=.*\d)[0-9 +()/\-]{7,30}$", RegexOptions.Compiled);

   private static readonly Regex OptionalTrunkZero =
      new(@"\(\s*0\s*\)", RegexOptions.Compiled);
}