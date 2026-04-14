using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class AccountDbContextEf(
   AppDbContext db
) : IAccountDbContext {
   
   public IQueryable<Account> Accounts => db.Set<Account>();
   public IQueryable<Beneficiary> Beneficiaries => db.Set<Beneficiary>();
   
   public void Add<T>(T entity) where T : class 
      => db.Set<T>().Add(entity);
   public void AddRange<T>(IEnumerable<T> entities) where T : class 
      => db.Set<T>().AddRange(entities);
   
   public void Remove(Beneficiary b) 
      => db.Set<Beneficiary>().Remove(b);
}