namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;

public interface IClock {
   DateTime UtcNow { get; }
}