using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest.TestInfrastructure;

public sealed class FakeClock : IClock {
   public static readonly DateTime DefaultUtcNow = DateTime.Parse("2020-01-01T00:00:00Z");

   public DateTime UtcNow { get; }

   public FakeClock() : this(DefaultUtcNow) {
   }

   public FakeClock(DateTime utcNow) {
      UtcNow = utcNow;
   }
}