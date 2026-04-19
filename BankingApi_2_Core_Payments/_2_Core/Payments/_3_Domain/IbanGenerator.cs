using System.Security.Cryptography;
using System.Text;

namespace BankingApi._2_Core.Payments;

public static class IbanGenerator {

   // Expected IBAN lengths (optional guard)
   private static readonly IReadOnlyDictionary<string, int> IbanLengths =
      new Dictionary<string, int>(StringComparer.Ordinal) {
         ["DE"] = 22,
         ["AT"] = 20,
         ["CH"] = 21,
      };

   // Build random DE IBAN for tests/demo
   public static string Build() {
      const string countryCode = "DE";

      var bban = new string(
         Enumerable.Range(0, 18)
            .Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(0, 10)))
            .ToArray());

      return Build(countryCode, bban);
   }

   // Builds a full IBAN from country code + BBAN by computing check digits.
   public static string Build(string countryCode, string bban) {
      countryCode = NormalizeCountry(countryCode);
      bban = NormalizeAlnum(bban);

      ValidateCountry(countryCode);

      var checkDigits = ComputeCheckDigits(countryCode, bban);
      var iban = countryCode + checkDigits + bban;

      ValidateIbanLength(countryCode, iban, nameof(bban));
      return iban;
   }

   // Replaces "XX" check digit placeholder in an IBAN template.
   public static string FillTemplateStart(string ibanWithXX) {
      var normalized = NormalizeAlnum(ibanWithXX);

      if (normalized.Length < 6)
         throw new ArgumentException("Template is too short.", nameof(ibanWithXX));

      var countryCode = normalized[..2];
      var placeholder = normalized.Substring(2, 2);
      var bban = normalized[4..];

      if (!string.Equals(placeholder, "XX", StringComparison.Ordinal))
         throw new ArgumentException(
            "Template must contain 'XX' at positions 3-4 (check digits).",
            nameof(ibanWithXX));

      return Build(countryCode, bban);
   }

   // Direct computation of last 2 digits (xx) without loop
   public static string FillTemplateEnd(
      string checkDigits,     // 2 digits
      string bankCode,        // 8 digits of BLZ
      string accountPrefix8   // 8 digits of account
   ) {
      bankCode = NormalizeDigits(bankCode);
      accountPrefix8 = NormalizeDigits(accountPrefix8);

      if (bankCode.Length != 8)
         throw new ArgumentException("Bank code must be exactly 8 digits.", nameof(bankCode));

      if (accountPrefix8.Length != 8)
         throw new ArgumentException("Account prefix must be exactly 8 digits.", nameof(accountPrefix8));

      // A = BLZ + account prefix
      var A = bankCode + accountPrefix8;

      // --- math part (mod 97) ---
      var aMod = Mod97Digits(A);

      var factor100 = 100 % 97;           // = 3
      var pow10_6 = PowMod(10, 6, 97);    // = 64
      var tail = Mod97Digits("131470");   // "DE70"

      var k = ((aMod * factor100) % 97 * pow10_6) % 97;
      k = (k + tail) % 97;

      var rhs = (1 - k) % 97;
      if (rhs < 0) rhs += 97;

      var inv64 = ModInverse(64, 97);

      var xx = (rhs * inv64) % 97;

      var suffix = xx.ToString("D2");

      return $"DE{checkDigits}{bankCode}{accountPrefix8}{suffix}";
   }

   // ------------------------------------------------------------
   // Computes IBAN check digits
   // ------------------------------------------------------------
   private static string ComputeCheckDigits(string countryCode, string bban) {
      countryCode = NormalizeCountry(countryCode);
      bban = NormalizeAlnum(bban);

      var prepared = bban + countryCode + "00";
      var mod = Mod97(prepared);
      var cd = 98 - mod;

      return cd.ToString("00");
   }

   // Streaming MOD-97 for alphanumeric input
   private static int Mod97(string alnum) {
      var mod = 0;

      foreach (char c in alnum) {
         if (c >= '0' && c <= '9') {
            mod = (mod * 10 + (c - '0')) % 97;
         }
         else if (c >= 'A' && c <= 'Z') {
            var val = (c - 'A') + 10;
            mod = (mod * 10 + (val / 10)) % 97;
            mod = (mod * 10 + (val % 10)) % 97;
         }
         else {
            throw new ArgumentException($"Invalid character '{c}'. Only A-Z and 0-9 are allowed.");
         }
      }

      return mod;
   }

   // --- helpers for math version (digits only) ---

   private static int Mod97Digits(string digits) {
      var mod = 0;
      foreach (var c in digits)
         mod = (mod * 10 + (c - '0')) % 97;
      return mod;
   }

   private static int PowMod(int baseVal, int exp, int mod) {
      var result = 1;
      for (var i = 0; i < exp; i++)
         result = (result * baseVal) % mod;
      return result;
   }

   private static int ModInverse(int a, int mod) {
      int t = 0, newT = 1;
      int r = mod, newR = a;

      while (newR != 0) {
         var q = r / newR;

         (t, newT) = (newT, t - q * newT);
         (r, newR) = (newR, r - q * newR);
      }

      if (r > 1)
         throw new InvalidOperationException("No inverse exists.");

      if (t < 0)
         t += mod;

      return t;
   }

   // ------------------------------------------------------------

   private static void ValidateCountry(string countryCode) {
      if (countryCode.Length != 2)
         throw new ArgumentException("Country code must be exactly 2 letters.", nameof(countryCode));

      if (!IbanLengths.ContainsKey(countryCode))
         throw new ArgumentException(
            $"Country '{countryCode}' not allowed (expected: {string.Join(", ", IbanLengths.Keys)}).",
            nameof(countryCode));
   }

   private static void ValidateIbanLength(string countryCode, string iban, string paramName) {
      var expectedLen = IbanLengths[countryCode];
      if (iban.Length != expectedLen)
         throw new ArgumentException(
            $"IBAN length for '{countryCode}' must be {expectedLen}, but was {iban.Length}. Check your BBAN length.",
            paramName);
   }

   private static string NormalizeCountry(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return string.Empty;

      return input.Trim().ToUpperInvariant();
   }

   private static string NormalizeAlnum(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return string.Empty;

      var sb = new StringBuilder(input.Length);

      foreach (var ch in input) {
         if (char.IsWhiteSpace(ch) || ch == '-' || ch == '.' || ch == '_')
            continue;

         sb.Append(char.ToUpperInvariant(ch));
      }

      return sb.ToString();
   }

   private static string NormalizeDigits(string? input) {
      var normalized = NormalizeAlnum(input);

      if (!normalized.All(char.IsDigit))
         throw new ArgumentException("Value must contain digits only.");

      return normalized;
   }
}