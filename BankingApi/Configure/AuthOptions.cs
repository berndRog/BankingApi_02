namespace BankingApi.Configure;

public sealed class AuthOptions {
   public string Authority { get; init; } = "";
   public string? Audience { get; init; }
   public bool RequireHttpsMetadata { get; init; } = true;
   public bool ValidateAudience { get; init; } = true;
   public int ClockSkewSeconds { get; init; } = 60;
}
