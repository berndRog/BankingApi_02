using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApi._3_Infrastructure._5_Utils;

public sealed class BankingSystemClock : IClock {
   public DateTime UtcNow => DateTime.UtcNow;
}