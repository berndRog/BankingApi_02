using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks.Utils;

public static class GuidExtensions {
   
   /// <summary>
   /// Returns a short log-friendly representation of a Guid
   /// (first 8 hex characters).
   /// </summary>
   public static string To8(this Guid value) =>
      value.ToString("N")[..8];

   /// <summary>
   /// Tries to parse a Guid from a string.
   /// </summary>
   public static Guid ToGuid(this string value) {
      return Guid.TryParse(value, out var guid) 
         ? guid 
         : throw new FormatException("Invalid Guid format");
   }
   /// <summary>
   /// Tries to parse a Guid from a string.
   /// </summary>
   public static Result<Guid> ToResultGuid(this string value) {
      return Guid.TryParse(value, out var guid) 
         ? Result<Guid>.Success(guid) 
         : Result<Guid>.Failure(DomainErrors.InvalidGuidFormat);
   }
}