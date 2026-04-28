using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcDeactivateIntT : TestBaseIntegration {
   
   public CustomerUcDeactivateIntT() {
      this.DbName = nameof(CustomerUcDeactivateIntT);
      this.DbMode = DbMode.FileUnique;
      this.SensitiveDataLogging = true;
   }
   
   
   [Fact]
   public async Task Deactivate_Customer_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var ucCreate = scope.ServiceProvider.GetRequiredService<CustomerUcCreate>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcDeactivate>();
      
      // Arrange
      var customer = seed.CustomerRegister(); // with address
      var customerCreateDto = customer.ToCustomerCreateDto(); 
      var account1 = seed.Account1(); 
      
      customerCreateDto = customerCreateDto with {
         AccountId = account1.Id.ToString(),
         Iban = account1.IbanVo.Value,
         Balance = account1.BalanceVo.Amount
      }; // we set the id to be the same as the seeded customer, so we can assert it later
      
      var resultCreate = await ucCreate.ExecuteAsync(
         customerCreateDto: customerCreateDto,
         ct
      );
      True(resultCreate.IsSuccess);
      unitOfWork.ClearChangeTracker();

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         customerId: customer.Id,
         ct
      );
      True(resultDeactivate.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actualCustomer = await customerRepository.FindByIdAsync(customer.Id, ct);
      NotNull(actualCustomer);
      Equal(customer.Id, actualCustomer.Id);
      Equal(customer.Firstname, actualCustomer.Firstname);
      Equal(customer.Lastname, actualCustomer.Lastname);
      Equal(customer.EmailVo, actualCustomer.EmailVo);
      Equal(customer.Subject, actualCustomer.Subject);
      Equal(customer.AddressVo, actualCustomer.AddressVo);
      
      var actualAccounts = await accountRepository.SelelctByCustomerIdAsync(customer.Id, ct);
      NotNull(actualAccounts);
      Single(actualAccounts);
      var actualAccount = actualAccounts.First();
      Equal(account1.Id,  actualAccount.Id); 
      Equal(account1.IbanVo, actualAccount.IbanVo);
      Equal(account1.BalanceVo, actualAccount.BalanceVo);
   }
}