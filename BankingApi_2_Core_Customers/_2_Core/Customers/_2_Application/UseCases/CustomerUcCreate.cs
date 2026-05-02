using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
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
   IEmployeeContract employeeContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcCreate> logger
) {
   public async Task<Result<CustomerDto>> ExecuteAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct = default
   ) {
      if(customerCreateDto == default)
         return Result<CustomerDto>.Failure(CustomerErrors.CustomerCreateDtoRequired);
      
      // 1) Load authorized employee and check if has rights to manage accounts
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(
         AdminRights.ManageAccounts, ct);   
      if(resultEmployee.IsFailure)
         return Result<CustomerDto>.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 2) subject required
      var resultSubject = IdentitySubject.Check(customerCreateDto.Subject);
      if (resultSubject.IsFailure) 
         return Result<CustomerDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;
      
      // 3) DomainModel
      // create email value object (domain logic inside)
      var resultDtoEmail = EmailVo.Create(customerCreateDto.Email);
      if (resultDtoEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultDtoEmail.Error);
      var emailDtoVo = resultDtoEmail.Value;
      // check email uniqueness
      if (await repository.FindByEmailAsync(emailDtoVo, ct) != null) {
         return Result<CustomerDto>.Failure(CustomerErrors.EmailAlreadyInUse);
      }
      
      // validate address if provided and create AddressVo
      var addressDto = customerCreateDto.AddressDto;
      var resultAddress = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );
      if (resultAddress.IsFailure)
         return Result<CustomerDto>.Failure(resultAddress.Error);
      var addressVo = resultAddress.Value;
      
      // create Customer entity using factory method and activate Customer 
      var result = Customer.Create(
         firstname: customerCreateDto.Firstname, 
         lastname: customerCreateDto.Lastname,  
         companyName: customerCreateDto.CompanyName, 
         subject: subject, 
         emailVo: emailDtoVo,
         addressVo: addressVo,
         createdAt: clock.UtcNow,
         id: customerCreateDto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<CustomerDto>.Failure(result.Error);
      var customer = result.Value;
      
      // 4) Check if there are accounts for this customer,
      // if so, fail (this is a severe error)
      var resultHasAccounts = await accountContract.HasNoAccountsAsync(customer.Id, ct);
      if (resultHasAccounts.IsFailure)
         return Result<CustomerDto>.Failure(resultHasAccounts.Error);
      var hasNoAccounts = resultHasAccounts.Value;
      if (!hasNoAccounts)
         return Result<CustomerDto>.Failure(CustomerErrors.AlreadyHasAccounts);
      
      // 5) Add customer to repository (tracked by EF)
      repository.Add(customer);
     
      // 6) Save all changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Create Customer", ct);
      logger.LogInformation("CustomerUcCreate={id} rows={rows}", customer.Id, rows);
      
      // 7) Activate customer
      customer.Activate(
         auditeddByEmployeeId: employeeContractDto.Id,
         activatedAt: clock.UtcNow
      );
      
      // 8) Save all changes to database
      rows = await unitOfWork.SaveAllChangesAsync("Activate Customer", ct);
      logger.LogInformation("CustomerUcCreate={id} rows={rows}", customer.Id, rows);
      
      // 9) Create initial account for customer 
      var resultAccount = await accountContract.OpenInitialAccountAsync(
         customerId: customer.Id,
         accountId: customerCreateDto.AccountId,
         iban: customerCreateDto.Iban,
         balance: customerCreateDto.Balance ?? 0.0m,
         ct: ct
      );
      if (resultAccount.IsFailure)
         return Result<CustomerDto>.Failure(resultAccount.Error);
            
      logger.LogInformation("CustomerUcCreate done CustomerId={id}, iban={iban}",
         customer.Id, resultAccount.Value.Iban);  
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}