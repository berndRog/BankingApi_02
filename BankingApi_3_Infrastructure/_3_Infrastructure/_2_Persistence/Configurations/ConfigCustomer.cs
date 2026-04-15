using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigCustomer(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Customer> {

   public void Configure(EntityTypeBuilder<Customer> builder) {
      
      // Tablename
      builder.ToTable("Customers");

      // Primary Key will never be generated
      builder.HasKey(o => o.Id);
      builder.Property(o => o.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);
      
      // Profile data
      builder.Property(o => o.Firstname)
         .HasMaxLength(80)
         .HasColumnName("Firstname").HasColumnOrder(1)
         .IsRequired(); 
      
      builder.Property(o => o.Lastname)
         .HasMaxLength(80)
         .HasColumnName("Lastname").HasColumnOrder(2)
         .IsRequired();
      
      builder.Property(o => o.CompanyName)
         .HasMaxLength(80)
         .HasColumnName("CompanyName").HasColumnOrder(3)
         .IsRequired(false);

      // Value Object EmailVo mit Conversion
      builder.Property(c => c.EmailVo)
         .HasConversion(vo => vo.Value, s => EmailVo.FromPersisted(s))
         .HasMaxLength(254)
         .HasColumnName("Email").HasColumnOrder(4)
         .IsRequired();
      builder.HasIndex(c => c.EmailVo).IsUnique();

      // Status
      builder.Property(o => o.Status)
         .HasConversion<int>()
         .HasColumnName("Status").HasColumnOrder(5)
         .IsRequired();
      
      builder.Property(o => o.Subject)
         .HasMaxLength(200)
         .HasColumnName("Subject").HasColumnOrder(6)
         .IsRequired();
      builder.HasIndex(o => o.Subject).IsUnique();
      
      // Address (owned value object)
      builder.OwnsOne(o => o.AddressVo, a => {
         
         a.Property(p => p.Street)
            .HasMaxLength(80)
            .HasColumnName("Street").HasColumnOrder(7)
            .IsRequired();

         a.Property(p => p.PostalCode)
            .HasMaxLength(20)
            .HasColumnName("PostalCode").HasColumnOrder(8)
            .IsRequired();

         a.Property(p => p.City)
            .HasMaxLength(80)
            .HasColumnName("City").HasColumnOrder(9)
            .IsRequired();

         a.Property(p => p.Country)
            .HasMaxLength(80)
            .HasColumnName("Country").HasColumnOrder(10)
            .IsRequired(false);
      });
      builder.Navigation(o => o.AddressVo).IsRequired();
      
      // Employee decisions / audit facts
      builder.Property(o => o.AuditedByEmployeeId)
         .HasColumnName("AuditedByEmployeeId").HasColumnOrder(11)
         .IsRequired(false);
      
      builder.Property(o => o.ActivatedAt)
         .HasConversion(dtConvNul)
         .HasColumnName("ActivatedAt").HasColumnOrder(12)
         .IsRequired(false);

      builder.Property(o => o.RejectedAt)
         .HasConversion(dtConvNul)
         .HasColumnName("RejectedAt").HasColumnOrder(13)
         .IsRequired(false);

      builder.Property(o => o.CustomerRejectCode)
         .HasConversion<int>()   
         .HasColumnName("RejectCode").HasColumnOrder(14)
         .IsRequired();

      builder.Property(o => o.DeactivatedByEmployeeId)
         .HasColumnName("DeactivatedByEmployeeId").HasColumnOrder(15)
         .IsRequired(false);
      
      builder.Property(o => o.DeactivatedAt)
         .HasConversion(dtConvNul)
         .HasColumnName("DeactivatedAt").HasColumnOrder(16)
         .IsRequired(false);
      
      // Auditing timestamps
      builder.Property(o => o.CreatedAt)
         .HasConversion(dtConv)
         .HasColumnName("CreatedAt").HasColumnOrder(17)
         .IsRequired();
      
      builder.Property(o => o.UpdatedAt)
         .HasConversion(dtConv)
         .HasColumnName("UpdatedAt").HasColumnOrder(18)
         .IsRequired();
      
      // Domain-only
      builder.Ignore(o => o.DisplayName);
      builder.Ignore(o => o.IsActive);
      builder.Ignore(o => o.IsProfileComplete);

      // Optional indexes for admin filtering
      builder.HasIndex(o => o.CreatedAt);
   }
}