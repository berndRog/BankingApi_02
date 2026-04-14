using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace BankingApiTest.TestController;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
   public const string SchemeName = "TestScheme";
   public const string Header = "X-Test-Roles"; // z.B. "Customer" oder "Employee" oder "Customer,Employee"

   public TestAuthHandler(
      IOptionsMonitor<AuthenticationSchemeOptions> options,
      ILoggerFactory logger,
      UrlEncoder encoder
   ) : base(options, logger, encoder) {
   }

   protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
      // wenn Header fehlt -> "nicht eingeloggt"
      if (!Request.Headers.TryGetValue(Header, out var rolesRaw))
         return Task.FromResult(AuthenticateResult.NoResult());

      var roles = rolesRaw.ToString()
         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      var claims = new List<Claim> {
         new(ClaimTypes.NameIdentifier, "test-user"),
         new(ClaimTypes.Name, "Test User"),
      };

      // Rollen als Role-Claims
      claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

      var identity = new ClaimsIdentity(claims, Scheme.Name);
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, Scheme.Name);

      return Task.FromResult(AuthenticateResult.Success(ticket));
   }
}