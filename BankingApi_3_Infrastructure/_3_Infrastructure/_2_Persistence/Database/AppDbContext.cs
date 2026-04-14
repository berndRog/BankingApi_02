using System.Runtime.CompilerServices;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Configurations;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {
  
   public DbSet<Customer> Customers => Set<Customer>();
   public DbSet<Account> Accounts => Set<Account>();
   public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      // Reuse converter instances (stateless, deterministic).
      var dtConv = new DateTimeOffsetToIsoStringConverter();
      var dtConvNul = new DateTimeOffsetToIsoStringConverterNullable();
      
      // Apply entity mappings (aggregate roots first).
      modelBuilder.ApplyConfiguration(new ConfigCustomer(dtConv, dtConvNul));
      
      modelBuilder.ApplyConfiguration(new ConfigAccount(dtConv));
      modelBuilder.ApplyConfiguration(new ConfigBeneficiary());
   }

}