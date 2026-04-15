using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

internal sealed class CustomerUcCreate(
   ICustomerRepository repository,
   IAccountContract accountContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcCreate> logger
) {
   public async Task<Result<CustomerDto>> ExecuteAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct = default
   ) {
      
      // 1) Load authorized employee and check if has rights to manage accounts
      // var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
      //    AdminRights.ManageAccounts, ct);   
      // if(resultEmployee.IsFailure)
      //   return Result<AccountDto>.Failure(resultEmployee.Error);
      // var employeeContractDto = resultEmployee.Value;
      var employeeContractDto = new EmployeeContractDto(
         Id: Guid.Parse("00000000-0002-0000-0000-000000000000"),
         AdminRightsInt: 511 // all AdminRights
      );
      
      // 2) DomainModel
      // create email value object (domain logic inside)
      var resultDtoEmail = EmailVo.Create(customerCreateDto.Email);
      if (resultDtoEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultDtoEmail.Error);
      var emailDtoVo = resultDtoEmail.Value;
      
      // check email uniqueness
      if (await repository.FindByEmailAsync(emailDtoVo, ct) != null) {
         return Result<CustomerDto>.Failure(CustomerErrors.EmailAlreadyInUse);
      }
      
      // create aggregate (domain logic inside)
      var result = Customer.Create(
         firstname: customerCreateDto.Firstname, 
         lastname: customerCreateDto.Lastname,  
         companyName: customerCreateDto.CompanyName, 
         emailVo: emailDtoVo,
         subject: customerCreateDto.Subject, 
         auditedByEmployeeId: employeeContractDto.Id,
         createdAt: clock.UtcNow,
         id: customerCreateDto.Id.ToString(),
         addressVo: customerCreateDto.AddressDto.ToAddressVo()
      );
      
      if (result.IsFailure) 
         return Result<CustomerDto>.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcCreate.DomainRejected",
               new { customerDto = customerCreateDto });
      var customer = result.Value!;
      
      // Check if there are accounts for this customer,
      // if so, fail (this is a severe error)
      var resultHasAccounts = await accountContract.HasNoAccountsAsync(customer.Id, ct);
      if (resultHasAccounts.IsFailure)
         return Result<CustomerDto>.Failure(resultHasAccounts.Error)
            .LogIfFailure(logger, "CustomerUcCreate.HasAccountsCheckFailed", new { customerId = result.Value!.Id });
      var hasNoAccounts = resultHasAccounts.Value;
      if (!hasNoAccounts)
         return Result<CustomerDto>.Failure(CustomerErrors.AlreadyHasAccounts);
      //
      // Add customer to repository (tracked by EF)
      repository.Add(customer);
     
      // Save all changes to database using a transaction
      var rows = await unitOfWork.SaveAllChangesAsync("Create Customer", ct);
      logger.LogInformation("CustomerUcCreate={id} rows={rows}", customer.Id, rows);
      
      // Create initial account for owner (domain logic in accounts module)
      var resultAccount = await accountContract.OpenInitialAccountAsync(
         customerId: customer.Id,
         accountId: customerCreateDto.AccountId,
         iban: customerCreateDto.Iban,
         balance: customerCreateDto.Balance ?? 0.0m,
         ct: ct
      );
      if(resultAccount.IsFailure)
         return Result<CustomerDto>.Failure(resultAccount.Error)
            .LogIfFailure(logger, "CustomerUcCreate.OpenInitialAccountFailed", new { customerId = customer.Id });
     
      logger.LogInformation("CustomerUcCreate done CustomerId={id}, iban={iban}",
         customer.Id, resultAccount.Value!.Iban);  
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}