namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;

public interface IIdentityGateway {
   string Subject { get; }           // OIDC: "sub" (stabil)
   string Username { get; }          // OIDC: "preferred_username" (= Login / initial email)
   DateTimeOffset CreatedAt { get; } // Identity creation time (stabil)
   int AdminRights { get; }          // bitmask claim "admin_rights" (0 for employees/customers, 1 for employees)
}

