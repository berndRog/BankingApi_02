using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcUpdateIntT : TestBaseIntegration {
   
   public CustomerUcUpdateIntT() {
      this.DbName = nameof(CustomerUcUpdateIntT);
      this.DbMode = DbMode.FileUnique;
      this.SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task Update_Customer_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
         
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var ucCreate = scope.ServiceProvider.GetRequiredService<CustomerUcCreate>();
      var sut  = scope.ServiceProvider.GetRequiredService<CustomerUcUpdate>();
      
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
      var customerUpdateDto = new CustomerUpdateDto(
         Lastname: "Meier-Engel",
         CompanyName: null,
         Email: "e.meier-engel@freenet.de",
         AddressDto: new AddressDto(
            Street: "Am Sande 17",
            PostalCode: "21335",
            City: "Lüneburg",
            Country: "DE"
         )
      );
      
      var resultUpdate = await sut.ExecuteAsync(
         customerId: customer.Id,
         customerUpdateDto: customerUpdateDto,
         ct
      );
      True(resultUpdate.IsSuccess);
      
      // Assert
      var actualCustomer = await customerRepository.FindByIdAsync(customer.Id, ct);
      NotNull(actualCustomer);
      Equal(customer.Id, actualCustomer.Id);
      Equal(customer.Firstname, actualCustomer.Firstname);
      Equal(customerUpdateDto.Lastname, actualCustomer.Lastname);
      Equal(customerUpdateDto.CompanyName, actualCustomer.CompanyName);
      Equal(customerUpdateDto.Email, actualCustomer.EmailVo.Value);
      Equal(customerUpdateDto.AddressDto?.Street, actualCustomer.AddressVo.Street);
      Equal(customerUpdateDto.AddressDto?.PostalCode, actualCustomer.AddressVo.PostalCode);
      Equal(customerUpdateDto.AddressDto?.City, actualCustomer.AddressVo.City);
      Equal(customerUpdateDto.AddressDto?.Country, actualCustomer.AddressVo.Country);
   }
}