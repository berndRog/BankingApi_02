namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;

public interface IClock {
   DateTimeOffset UtcNow { get; }
}