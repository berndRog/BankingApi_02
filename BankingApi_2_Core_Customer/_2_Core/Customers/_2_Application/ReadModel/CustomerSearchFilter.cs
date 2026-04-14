namespace BankingApi._2_Core.Customers._2_Application.ReadModel;

/// <summary>
/// Filter options for customer search endpoints.
///
/// Purpose:
/// - Used by controllers to express UI-driven query criteria
/// - Can be combined freely (AND semantics)
/// - Must be translated into database queries (EF Core)
///
/// Important:
/// - This is a read-model filter, not a domain object
/// - No business rules or invariants are enforced here
/// </summary>
public sealed record CustomerSearchFilter(
   string? Email = null,
   string? Firstname = null,
   string? Lastname = null
);
