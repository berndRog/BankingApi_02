using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModels;

internal sealed class CustomerReadModelEf(
   ICustomerDbContext customerDbContext
): ICustomerReadModel {

   public async Task<Result<CustomerDto>> FindByIdAsync(
      Guid id, 
      CancellationToken ct = default
   ) {
      var customerDto = await customerDbContext.Customers
         .AsNoTracking()                      // don't save to repo   
         .Where(c => c.Id == id)              // filter
         .Select(c => c.ToCustomerDto())      // map ToCustomerDto() 
         .SingleOrDefaultAsync(ct);           // terminal operation
      
       return customerDto is null 
          ? Result<CustomerDto>.Failure(CustomerErrors.NotFound)
          : Result<CustomerDto>.Success(customerDto);
   }

   public async Task<Result<CustomerDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var resultEmail = EmailVo.Create(email);
      if(resultEmail.IsFailure) 
         return Result<CustomerDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;
      
      var customerDto = await customerDbContext.Customers
         .AsNoTracking()                      // don't save to repo   
         .Where(c => c.EmailVo == emailVo)    // filter
         .Select(c => c.ToCustomerDto())      // map ToCustomerDto() 
         .SingleOrDefaultAsync(ct);           // terminal operation
      
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
         .AsNoTracking()                      // don't save to repo   
         .Where(c =>                          // filter SQL like %..% 
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