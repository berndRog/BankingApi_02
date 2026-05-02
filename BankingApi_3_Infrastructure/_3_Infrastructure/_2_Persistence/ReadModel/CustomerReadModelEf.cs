using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;

internal sealed class CustomerReadModelEf(
   ICustomerDbContext customerDbContext,
   IIdentityGateway identityGateway
) : ICustomerReadModel {
   
   public async Task<Result<CustomerDto>> FindMeAsync(CancellationToken ct) {
      // 1) Subject from Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<CustomerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Customer by subject (NO tracking, read-only)
      var customerDto = await customerDbContext.Customers
         .AsNoTracking()
         .Where(c => c.Subject == subject)    // filter by subject
         .Select(c => c.ToCustomerDto())      // project to CustomerDto (map)
         .SingleOrDefaultAsync(ct);
      
      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerErrors.NotProvisioned)   
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<CustomerDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var customerDto = await customerDbContext.Customers
         .AsNoTracking()
         .Where(c => c.Id == Id)       // filter by Id
         .Select(c => c.ToCustomerDto())  // project to CustomerDto (map)
         .SingleOrDefaultAsync(ct);

      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerErrors.NotFound)
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<CustomerDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {

      var result = EmailVo.Create(email);
      if (result.IsFailure)      
         return Result<CustomerDto>.Failure(result.Error);
      var emailVo = result.Value;
      
      var customerDto = await customerDbContext.Customers
         .AsNoTracking()
         .Where(c => c.EmailVo == emailVo) // filter by email
         .Select(c => c.ToCustomerDto())  // projection to CustomerDto
         .SingleOrDefaultAsync( ct);
      
      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerErrors.NotFound)
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<IEnumerable<CustomerDto>>> SelectByDisplayNameAsync(
      string displayName,
      CancellationToken ct = default
   ) {
      var pattern = $"%{displayName}%";
      var customerDtos = await customerDbContext.Customers
         .Where(c =>
            EF.Functions.Like(
               c.CompanyName ?? c.Firstname + " " + c.Lastname,
               pattern))
         .Select(c => c.ToCustomerDto())
         .ToListAsync(ct);
      return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
   }
   
   public async Task<Result<IEnumerable<CustomerDto>>> SelectAllAsync(
      CancellationToken ct
   ) {
      var customerDtos = await customerDbContext.Customers
         .AsNoTracking()
         .Select(c => c.ToCustomerDto()) // project to CustomerDto (map)
         .ToListAsync(ct);
      return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
   }
}
