using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestController;
using BankingApiTest.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class AccountsControllerEndToEnd : TestBaseEndToEnd {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region PostAccount_Create
   [Fact]
   public async Task PostAccount_Create_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var customerId = customer1.Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         db.Customers.Add(customer1);
         db.Accounts.Add(account1);
         await db.SaveChangesAsync(ct);
      });
      
      // Act
      var requestAccountDto = new AccountDto(
         Id: account2.Id,
         Iban: account2.IbanVo.Value,
         Balance: account2.BalanceVo.Amount,
         Currency: (int)account2.BalanceVo.Currency, // "EUR",
         CustomerId: account2.CustomerId
      );
      //  [HttpPost("customers/{customerId:guid}/accounts")]
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri: $"/banking/v2/customers/{customerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(requestAccountDto);

      var responsePostAccount = await Client.SendAsync(request, ct);

      var account2Dto = 
         await responsePostAccount.Content.ReadFromJsonAsync<AccountDto>(ct); 
      NotNull(account2Dto);
      Equal(account2.Id, account2Dto.Id);
      Equal(account2.IbanVo.Value, account2Dto.Iban);
      Equal(account2.BalanceVo.Amount, account2Dto.Balance);
      Equal((int)account2.BalanceVo.Currency, account2Dto.Currency);
      Equal(account2.CustomerId, account2Dto.CustomerId);

      // Assert (HTTP)
      True(
         condition: responsePostAccount.StatusCode is HttpStatusCode.Created,
         userMessage:
         $"Unexpected status {(int)responsePostAccount.StatusCode} {responsePostAccount.StatusCode}\n{account2Dto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         
         // Domain-level checks
         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);
         Equal(2, accounts.Count);
         
         var actualAccount2 = accounts[1];
         Equal(account2.Id, actualAccount2.Id);
         Equal(account2.IbanVo, actualAccount2.IbanVo);
         Equal(account2.BalanceVo.Amount, actualAccount2.BalanceVo.Amount);
         Equal(account2.BalanceVo.Currency, actualAccount2.BalanceVo.Currency);
         Equal(account2.CustomerId, actualAccount2.CustomerId);
      });
   }
   #endregion
   
   #region GetAccount_byId
   [Fact]
   public async Task GetAccountById() {
      var ct = TestContext.Current.CancellationToken; 
      
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var accountId = account2.Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
         // seed here...
         dbContext.Customers.Add(customer1);
         dbContext.Accounts.Add(account1);
         dbContext.Accounts.Add(account2);
         await unitOfWork.SaveAllChangesAsync("",ct);
      });
      
      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/accounts/{accountId}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetAccountbyId = await Client.SendAsync(request, ct);
      var accountDto = 
         await responseGetAccountbyId.Content.ReadFromJsonAsync<AccountDto>(ct); 
      NotNull(accountDto);
      
      Equal(account2.Id, accountDto.Id);
      Equal(account2.IbanVo.Value, accountDto.Iban);
      
      // Assert (HTTP)
      True(
         condition: responseGetAccountbyId.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseGetAccountbyId.StatusCode} {responseGetAccountbyId.StatusCode}\n{accountDto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         
         // Domain-level checks
         var actualAccount = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, ct);
         
         Equal(account2.Id, actualAccount?.Id);
         Equal(account2.IbanVo, actualAccount?.IbanVo);
         Equal(account2.BalanceVo.Amount, actualAccount?.BalanceVo.Amount);
         Equal(account2.BalanceVo.Currency, actualAccount?.BalanceVo.Currency);
         Equal(account2.CustomerId, actualAccount?.CustomerId);
      });
   }
   #endregion
   
   #region GetAccount_byIban
   [Fact]
   public async Task GetAccountByIban() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
         // seed here...
         dbContext.Customers.Add(customer1);
         dbContext.Accounts.Add(account1);
         dbContext.Accounts.Add(account2);
         await unitOfWork.SaveAllChangesAsync("",ct);
      });
      
      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/accounts/iban?iban={account2.IbanVo.Value}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetAccountbyId = await Client.SendAsync(request, ct);
      var accountDto = 
         await responseGetAccountbyId.Content.ReadFromJsonAsync<AccountDto>(ct);
      NotNull(accountDto);
      
      Equal(account2.Id, accountDto.Id);
      Equal(account2.IbanVo.Value, accountDto.Iban);
      
      // Assert (HTTP)
      True(
         condition: responseGetAccountbyId.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseGetAccountbyId.StatusCode} {responseGetAccountbyId.StatusCode}\n{accountDto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         
         // Domain-level checks
         var actualAccount = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IbanVo == account2.IbanVo, ct);
         
         Equal(account2.Id, actualAccount?.Id);
         Equal(account2.IbanVo, actualAccount?.IbanVo);
         Equal(account2.BalanceVo, actualAccount?.BalanceVo);
         Equal(account2.CustomerId, actualAccount?.CustomerId);
      });
   }
   #endregion
   
   #region GetAccounts_ByCustomerId
   [Fact]
   public async Task GetAccountsByCustomerId() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var Customers = _seed.Customers;
      var accounts = _seed.Accounts;
      var customerId = _seed.Customer1().Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>(); 
         dbContext.Customers.AddRange(Customers);
         dbContext.Accounts.AddRange(accounts);
         await unitOfWork.SaveAllChangesAsync("",ct);
      });
      
      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/banking/v2/customers/{customerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");

      var responseAllGetAccounts = await Client.SendAsync(request, ct);
      // Assert (HTTP)
      True(
         condition: responseAllGetAccounts.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseAllGetAccounts.StatusCode} {responseAllGetAccounts.StatusCode}\n"
      );
      var accountDtos = 
         await responseAllGetAccounts.Content.ReadFromJsonAsync<List<AccountDto>>(ct); // helpful for debugging
      NotNull(accountDtos);
      
      Equal(2, accountDtos.Count);
      var expectedIds = accounts
         .Where(a => a.CustomerId == customerId)
         .Select(a => a.Id)
         .OrderBy(id => id).ToList();
      var accountDtosIds  = accountDtos
         .Select(a => a.Id)
         .OrderBy(id => id).ToList();
      Equal(expectedIds, accountDtosIds);
      
      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         
         // Domain-level checks
         var actualAccountIds = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .Select(a => a.Id)
            .OrderBy(id => id)
            .ToListAsync(ct);
         Equal(expectedIds, actualAccountIds);

      });
   }
   #endregion
}