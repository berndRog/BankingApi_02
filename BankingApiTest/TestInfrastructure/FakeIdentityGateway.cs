using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest.TestInfrastructure;

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; }
   public string Username { get; }
   public DateTime CreatedAt { get; }
   public int AdminRights { get; }

   public FakeIdentityGateway(
      string subject,
      string username,
      DateTime createdAt,
      int? adminRights = null
   ) {
      Subject = subject;
      Username = username;
      CreatedAt = createdAt;
      if (adminRights.HasValue)
         AdminRights = adminRights.Value;
   }
}