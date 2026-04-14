using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class UnitOfWork(
   AppDbContext dbContext,
   IClock clock,
   ILogger<UnitOfWork> logger
) : IUnitOfWork {
   public async Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ) {
      dbContext.ChangeTracker.DetectChanges();
      DumpChangeTrackerToConsole(text);
      
      ApplyAuditInfo();
      var rows = await dbContext.SaveChangesAsync(ctToken);
      
      DumpChangeTrackerToConsole(text);
      return rows;
   }

   public void ClearChangeTracker() =>
      dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (!logger.IsEnabled(LogLevel.Debug)) return;
      DumpChangeTrackerToConsole(text);
   }
   
   // Audit
   // -----------------------------
   private void ApplyAuditInfo() {
      var now = clock.UtcNow;

      foreach (var entry in dbContext.ChangeTracker.Entries<AggregateRoot>()) {
         switch (entry.State) {
            case EntityState.Added:
               entry.Property(nameof(AggregateRoot.CreatedAt)).CurrentValue = now;
               entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = now;
               break;
            case EntityState.Modified:
               entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = now;
               break;
         }
      }
   }

   // Workaround - Logger is cutting output
   public void DumpChangeTrackerToConsole(string? text) {
      if (!logger.IsEnabled(LogLevel.Debug)) return;
      dbContext.ChangeTracker.DetectChanges();
      var output = dbContext.ChangeTracker.DebugView.LongView;
      Console.WriteLine($"{DateTime.Now:HH:mm:ss} DEBUG ChangeTracker:");
      if(text is not null) Console.WriteLine($"=== {text} ===");
      Console.WriteLine(output);
      Console.WriteLine("=== END ===");
   }
}