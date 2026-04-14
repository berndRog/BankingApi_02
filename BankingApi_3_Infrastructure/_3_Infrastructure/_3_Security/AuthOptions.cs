namespace BankingApi._3_Infrastructure._3_Security;

public sealed class AuthOptions {
   public string Authority { get; init; } = "";
   public string? Audience { get; init; }
   public bool RequireHttpsMetadata { get; init; } = true;
   public bool ValidateAudience { get; init; } = true;
   public int ClockSkewSeconds { get; init; } = 60;
}
