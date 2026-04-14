using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

internal sealed class ConfigCustomer(
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
         .HasColumnName("Id")
         .HasColumnOrder(0);
      
      // Profile data
      builder.Property(o => o.Firstname)
         .HasMaxLength(80)
         .HasColumnName("Firstname")
         .HasColumnOrder(1)
         .IsRequired();
      builder.Property(o => o.Lastname)
         .HasMaxLength(80)
         .HasColumnName("Lastname")
         .HasColumnOrder(2)
         .IsRequired();
      builder.Property(o => o.CompanyName)
         .HasMaxLength(80)
         .HasColumnName("CompanyName")
         .HasColumnOrder(3)
         .IsRequired(false);

      // Value Object EmailVo mit Conversion
      builder.Property(c => c.EmailVo)
         .HasConversion(vo => vo.Value, s => EmailVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("Email")
         .HasColumnOrder(4)
         .HasMaxLength(254);
      builder.HasIndex(c => c.EmailVo).IsUnique();

      // Status
      builder.Property(o => o.Status)
         .HasConversion<int>()
         .HasColumnName("Status")
         .HasColumnOrder(5)
         .IsRequired();
      
      builder.Property(o => o.Subject)
         .HasMaxLength(200)
         .HasColumnName("Subject")
         .HasColumnOrder(6)
         .IsRequired();
      builder.HasIndex(o => o.Subject).IsUnique();
      
      // Auditing timestamps
      builder.Property(o => o.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();
      // Employee decisions / audit facts
      builder.Property(o => o.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();
      
      builder.Property(o => o.ActivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.RejectedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.RejectCode)
         .HasConversion<int>()   
         .IsRequired();

      builder.Property(o => o.AuditedByEmployeeId)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedByEmployeeId)
         .IsRequired(false);

      // Domain-only
      builder.Ignore(o => o.DisplayName);
      builder.Ignore(o => o.IsActive);
      builder.Ignore(o => o.IsProfileComplete);
      

      // Address (owned value object)
      builder.OwnsOne(o => o.AddressVo, a => {
         
         a.Property(p => p.Street)
            .HasMaxLength(80)
            .HasColumnName("Street")
            .IsRequired();

         a.Property(p => p.PostalCode)
            .HasMaxLength(20)
            .HasColumnName("PostalCode")
            .IsRequired();

         a.Property(p => p.City)
            .HasMaxLength(80)
            .HasColumnName("City")
            .IsRequired();

         a.Property(p => p.Country)
            .HasMaxLength(80)
            .HasColumnName("Country")
            .IsRequired(false);
      });
      builder.Navigation(o => o.AddressVo).IsRequired();

      // Optional indexes for admin filtering
      builder.HasIndex(o => o.CreatedAt);
   }
}
