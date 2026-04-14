namespace BankingApi._2_Core.Customers._2_Application.ReadModel;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CustomerListItemDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsBlocked
);
