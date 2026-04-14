using System.Globalization;
namespace BankingApi._2_Core.BuildingBlocks.Utils;

public static class DateTimeOffsetExtensions {
   
   public static string ToDateTimeString(this DateTimeOffset dtOffset, bool asUtc = false) {
      var value = asUtc ? dtOffset.ToUniversalTime() : dtOffset;
      return value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
   }   
   
   public static DateTimeOffset UtcDateTimeOffset(
      int year, 
      int month, 
      int day, 
      int hour = 0, 
      int min = 0,
      int sec = 0
   ) {
      var utcDateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);
      return new DateTimeOffset(utcDateTime);
   }

   public static DateTimeOffset LocalToUtc(
      int year,
      int month,
      int day,
      int hour = 0,
      int min = 0,
      int sec = 0
   ) {
      var localDateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Local);
      return new DateTimeOffset(localDateTime).ToUniversalTime();
   }
   


}


