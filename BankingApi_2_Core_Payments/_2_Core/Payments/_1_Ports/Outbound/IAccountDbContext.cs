using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Persistence context abstraction for the Accounts part of the Payments context.
// Provides query access to Account aggregates and their Beneficiaries.
// Used by repositories to interact with the database without exposing EF Core.
public interface IAccountDbContext {

   // Query access to Account aggregates
   IQueryable<Account> Accounts { get; }

   // Query access to beneficiaries belonging to accounts
   IQueryable<Beneficiary> Beneficiaries { get; }
   
   // Add a new entity to the persistence context
   void Add<T>(T entity) where T : class;
   void AddRange<T>(IEnumerable<T> entities) where T : class;
   
   // Remove an entity from the persistence context
   void Remove(Beneficiary b);
}
