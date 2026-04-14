using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Database.Converter;

public sealed class DateTimeOffsetToIsoStringConverterNullable()
   : ValueConverter<DateTimeOffset?, string?>(
      dto => dto.HasValue
         ? dto.Value.ToUniversalTime()
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)
         : null,
      iso => string.IsNullOrWhiteSpace(iso)
         ? null
         : DateTimeOffset.Parse(
            iso,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
         )
   ); 

