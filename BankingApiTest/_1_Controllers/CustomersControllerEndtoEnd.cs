using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestController;
using BankingApiTest.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._1_Controllers;

public sealed class CustomersControllerEndtoEnd : TestBaseEndToEnd {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region Post_Customer_Create
   [Fact]
   public async Task PostCustomer_Create_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();

      var requestDto = new CustomerCreateDto(
         Id: customer1.Id,
         Firstname: customer1.Firstname,
         Lastname: customer1.Lastname,
         CompanyName: customer1.CompanyName,
         Email: customer1.EmailVo.Value,
         StatusInt: (int) customer1.Status,
         Subject : customer1.Subject,
         AddressDto: customer1.AddressVo.ToAddressDto(),
         AccountId: account1.Id.ToString(),
         Iban: account1.IbanVo.Value,
         Balance: account1.BalanceVo.Amount
      );
     
      // Act
      Factory.TestSubject = customer1.Subject;
      
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri:"/banking/v2.0/customers"
      );
      //request.Headers.Add(TestAuthHandler.Header, "Employee");
      request.Content = JsonContent.Create(requestDto);

      var response = await Client.SendAsync(request, ct);
      
      var customerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(customerDto);
      True(
         condition: response.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n{customerDto?.Id}"
      );

      // assert
      // Domain-level checks
      Equal(requestDto.Firstname, customerDto?.Firstname);
      Equal(requestDto.Lastname, customerDto?.Lastname);

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == customerDto!.Id)
            .SingleOrDefaultAsync(ct);

         NotNull(customer);

         // Domain-level checks
         Equal(requestDto.Firstname, customer.Firstname);
         Equal(requestDto.Lastname, customer.Lastname);
         Equal(requestDto.Email, customer.EmailVo.Value);
         Equal(requestDto.StatusInt, (int)customer.Status);
         Equal(Factory.TestSubject, customer.Subject);
         Equal(requestDto.AddressDto, customer.AddressVo.ToAddressDto());
         
         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerDto!.Id)
            .ToListAsync(ct);
         Equal(1, accounts?.Count); // exactly one account should be created
      });
   }
   #endregion

   #region Get_Customer_ById_and_Email
   [Fact]
   public async Task GetCustomer_ById_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var employees = _seed.Customers;
      //  var owner = employees[0];
      var customer = employees[1];

      // damit TestAuthHandler den
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         db.Customers.AddRange(employees);
         await db.SaveChangesAsync(ct);
      });

      // Act
      var id = customer.Id;

      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/customers/{id}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      var actualCustomerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(actualCustomerDto);

      Equals(customer.Id, actualCustomerDto?.Id);
      Equals(customer.Firstname, actualCustomerDto?.Firstname);
      Equals(customer.Lastname, actualCustomerDto?.Lastname);
      Equals(customer.CompanyName, actualCustomerDto?.CompanyName);
      Equals(customer.EmailVo, actualCustomerDto?.Email);
      Equals((int)customer.Status, actualCustomerDto?.StatusInt);
      //Equal(Factory.TestSubject, owner.Subject);
      Equals(customer.AddressVo, actualCustomerDto);
   }

   [Fact]
   public async Task GetOwner_ByEmail_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var customers = _seed.Customers;
      var customer1 = customers[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync(ct);
      });

      // Act
      var email = customer1.EmailVo;

      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/customers/email?email={email}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualCustomerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);

      Equals(customer1.Id, actualCustomerDto?.Id);
      Equals(customer1.Firstname, actualCustomerDto?.Firstname);
      Equals(customer1.Lastname, actualCustomerDto?.Lastname);
      Equals(customer1.CompanyName, actualCustomerDto?.CompanyName);
      Equals(customer1.EmailVo, actualCustomerDto?.Email);
      Equals((int)customer1.Status, actualCustomerDto?.StatusInt);
      Equals(customer1.AddressVo, actualCustomerDto);
   }
   #endregion

   #region Get_All_Customers
   [Fact]
   public async Task GetAllCustomers_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var customers = _seed.Customers;
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync(ct);
      });

      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/customers"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");

      var response = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualCustomersDtos = 
         await response.Content.ReadFromJsonAsync<List<CustomerDto>>(ct);

      Equal(customers.Count, actualCustomersDtos?.Count);
      
   }
   #endregion
}