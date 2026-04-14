using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class CustomerRepositoryIntT : TestBaseIntegration {

   [Fact]
   public async Task Add_customer_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customer = seed.Customer1();

      // Act
      repository.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Add a customer", ct);

      // Assert
      var actual = await repository.FindByIdAsync(customer.Id, ct);
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.AddressVo, actual.AddressVo);
   }


   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      var customers = seed.Customers;
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      unitOfWork.ClearChangeTracker();

      var customer = customers[0]; // Customer1
      var id = customer.Id;

      // Act
      var actual = await repository.FindByIdAsync(id, ct);

      // Assert
      NotNull(actual);
      Equal(id, actual.Id);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.AddressVo, actual.AddressVo);
   }
   
   
   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var dbContext = scope.ServiceProvider.GetRequiredService<ICustomerDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      var customers = seed.Customers;
      dbContext.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      unitOfWork.ClearChangeTracker();

      var customer = customers[2]; // Customer3
      var emailVo = customer.EmailVo;

      // Act
      var actual = await repository.FindByEmailAsync(emailVo, ct);

      // Assert
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.CompanyName, actual.CompanyName);
      Equal(customer.AddressVo, actual.AddressVo);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.AddressVo, actual.AddressVo);
   }
   
   [Fact]
   public async Task SelectAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      repository.AddRange(seed.Customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var customers = await repository.SelectAllAsync(ct);
      
      // Assert
      var actualIds = customers.Select(c => c.Id).OrderBy(id => id).ToList();
      var expectedIds = seed.Customers.Select(c => c.Id).OrderBy(id => id).ToList();
      Equal(6, actualIds.Count);
      Equal(expectedIds, actualIds);
   }
   
   [Fact]
   public async Task SelectByName_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      repository.AddRange(seed.Customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      unitOfWork.ClearChangeTracker();
      var expected = new List<Customer>() { seed.Customers[0], seed.Customers[1] };

      // Act
      var customers = await repository.SelectByDisplayNameAsync("Mustermann", ct);
      
      // Assert
      var actualIds = customers.Select(c => c.Id).OrderBy(id => id).ToList();
      var expectedIds = expected.Select(c => c.Id).OrderBy(id => id).ToList();
      Equal(2, actualIds.Count);
      Equal(expectedIds, actualIds);
   }
}