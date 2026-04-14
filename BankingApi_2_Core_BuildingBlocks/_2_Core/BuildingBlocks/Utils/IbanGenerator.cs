using System.Security.Cryptography;
namespace BankingApi._2_Core.BuildingBlocks.Utils;

public static class IbanGenerator {
   
   // Generate a valid Iban from scratch
   public static string CreateGermanIban() {
      // generate random 18-digit BBAN
      var bban = "DEXX" + Generate18Digits(); 
      
      // remove spaces
      bban = new string(bban.Where(char.IsDigit).ToArray());
      if (bban.Length != 18)
         throw new ArgumentException("German BBAN must be 18 digits");

      var country = "DE";
      var temp = bban + CountryToNumbers(country) + "00";

      var remainder = Mod97(temp);
      var check = 98 - remainder;

      return Format($"{country}{check:00}{bban}");
   }
   
   
   // Given DEXX 1000 0000 0000 0000 00,
   // we need to calculate the check digits (XX) to make it a valid IBAN.
   public static string CreateGermanIban(string bban) {
      // remove spaces
      bban = new string(bban.Where(char.IsDigit).ToArray());

      if (bban.Length != 18)
         throw new ArgumentException("German BBAN must be 18 digits");

      var country = "DE";
      var temp = bban + CountryToNumbers(country) + "00";

      var remainder = Mod97(temp);
      var check = 98 - remainder;

      return Format($"{country}{check:00}{bban}");
   }

   private static string CountryToNumbers(string country) {
      return string.Concat(country.Select(c => (c - 'A' + 10).ToString()));
   }

   private static int Mod97(string input) {
      var remainder = 0;
      foreach (var c in input)
         remainder = (remainder * 10 + (c - '0')) % 97;

      return remainder;
   }
   
   private static string Generate18Digits() {
      const int length = 18;
      Span<char> chars = stackalloc char[length];
      for (int i = 0; i < length; i++) {
         chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
      }
      return new string(chars);
   }
   
   private static string Format(string iban) {
      if (string.IsNullOrWhiteSpace(iban))
         return iban;

      // remove existing spaces
      iban = new string(iban.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

      return string.Join(" ",
         Enumerable.Range(0, (iban.Length + 3) / 4)
            .Select(i => iban.Substring(i * 4, Math.Min(4, iban.Length - i * 4))));
   }
   

}