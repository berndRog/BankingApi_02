using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest.TestInfrastructure;

public sealed class FakeClock : IClock {
   public static readonly DateTimeOffset DefaultUtcNow = DateTimeOffset.Parse("2020-01-01T00:00:00Z");

   public DateTimeOffset UtcNow { get; }

   public FakeClock() : this(DefaultUtcNow) {
   }

   public FakeClock(DateTimeOffset utcNow) {
      UtcNow = utcNow;
   }
}