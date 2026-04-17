using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

internal class CustomerRepositoryEf(
   ICustomerDbContext customerDbContext
) : ICustomerRepository {
   
   public async Task<Customer?> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   ) => await customerDbContext.Customers
         .AsTracking()  // load into repository for EF Tracking
         .SingleOrDefaultAsync(c => c.Id == customerId, ct);

   public async Task<Customer?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) => await customerDbContext.Customers
         .AsTracking()  // load into repository for EF Tracking
         .SingleOrDefaultAsync(c => c.Subject == subject, ct);

   public async Task<Customer?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct
   ) => await customerDbContext.Customers
         .AsTracking()  // load into repository for EF Tracking
         .SingleOrDefaultAsync(c => c.EmailVo == emailVo, ct);
   
   public async Task<bool> ExistsActiveAsync(
      Guid customerId,
      CancellationToken ct = default
   ) => await customerDbContext.Customers
      .AsTracking()  // load into repository for EF Tracking
      .Where(c => c.Id == customerId && c.DeactivatedAt == null)
      .AnyAsync(ct);
   
   public async Task<IReadOnlyList<Customer>> SelectByDisplayNameAsync(
      string displayName,
      CancellationToken ct = default
   ) {
      var pattern = $"%{displayName}%";
      return await customerDbContext.Customers
         .AsTracking()  // load into repository for EF Tracking
         .Where(c =>
            EF.Functions.Like(
               c.CompanyName ?? c.Firstname + " " + c.Lastname,
               pattern))
         .ToListAsync(ct);
   }
   
   public async Task<IReadOnlyList<Customer>> SelectAllAsync(
      CancellationToken ct = default
   ) => await customerDbContext.Customers
         .AsTracking()  // load into repository for EF Tracking   
         .ToListAsync(ct);

   public void Add(Customer customer)
      => customerDbContext.Add(customer);

   public void AddRange(IEnumerable<Customer> customers)
      => customerDbContext.AddRange(customers);

   public void Update(Customer customer)
      => customerDbContext.Update(customer);
}