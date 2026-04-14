namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;

public interface IUnitOfWork {
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}