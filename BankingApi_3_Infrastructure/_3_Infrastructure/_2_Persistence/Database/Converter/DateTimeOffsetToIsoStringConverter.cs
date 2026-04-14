using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Database.Converter;


public sealed class DateTimeOffsetToIsoStringConverter() 
   : ValueConverter<DateTimeOffset, string>(
      dto => dto.ToUniversalTime()
         .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
      iso => DateTimeOffset.Parse(
         iso,
         CultureInfo.InvariantCulture,
         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
      )
   ) {
}



/*
// epoch ist leider schlecht lesbar
public sealed class DateTimeOffsetToUnixTimeConverter() 
   : ValueConverter<DateTimeOffset, long>(
      // ➜ C# → DB (write)
      dto => dto.ToUnixTimeMilliseconds(),
      // ➜ DB → C# (read)  ← THIS is the inverse direction
      millis => DateTimeOffset.FromUnixTimeMilliseconds(millis)
) { }
*/