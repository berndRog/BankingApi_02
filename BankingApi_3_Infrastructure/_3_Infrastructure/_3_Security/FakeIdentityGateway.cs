using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApi._3_Infrastructure._3_Security;

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; }
   public string Username { get; }
   public DateTimeOffset CreatedAt { get; }
   public int AdminRights { get; }

   public FakeIdentityGateway(
      string subject,
      string username,
      DateTimeOffset createdAt,
      int? adminRights = null
   ) {
      Subject = subject;
      Username = username;
      CreatedAt = createdAt;
      if (adminRights.HasValue)
         AdminRights = adminRights.Value;
   }
}