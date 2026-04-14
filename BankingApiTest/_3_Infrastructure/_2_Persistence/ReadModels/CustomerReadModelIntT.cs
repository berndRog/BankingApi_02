using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class CustomerReadModelIntT : TestBaseIntegration {

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer = customers[0];
      var customerDto =  customer.ToCustomerDto();

      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      // Act
      var result = await readModel.FindByIdAsync(customerDto.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDto = result.Value;
      NotNull(actualDto);
      Equal(customerDto.Id, actualDto.Id);
      Equal(customerDto.Firstname, actualDto.Firstname);
      Equal(customerDto.Lastname, actualDto.Lastname);
      Equal(customerDto.CompanyName, actualDto.CompanyName);
      Equal(customerDto.StatusInt, actualDto.StatusInt);
      Equal(customerDto.Email, actualDto.Email);
      Equal(customerDto.AddressDto, actualDto.AddressDto);
   }
   
   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer = customers[0];
      var customerDto =  customer.ToCustomerDto();
      
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByEmailAsync(customerDto.Email, ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value;
      NotNull(actual);
      Equal(customerDto.Id, actual.Id);
      Equal(customerDto.Firstname, actual.Firstname);
      Equal(customerDto.Lastname, actual.Lastname);
      Equal(customerDto.CompanyName, actual.CompanyName);
      Equal(customerDto.StatusInt, actual.StatusInt);
      Equal(customerDto.Email, actual.Email);
      Equal(customerDto.AddressDto, actual.AddressDto);
   }
   
   [Fact]
   public async Task SelectByNameAsync_loads_Mustermann_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer1 = customers[0];
      var customer2 = customers[1];
      var customer1Dto =  customer1.ToCustomerDto();
      var customer2Dto =  customer2.ToCustomerDto();
      
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectByDisplayNameAsync("Mustermann", ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDtos = result.Value.ToList();
      Equal(2, actualDtos.Count());
      var actual1Dto = actualDtos[0];
      var actual2Dto = actualDtos[1];
      Equal(customer1Dto.Id, actual1Dto.Id);
      Equal(customer1Dto.Firstname, actual1Dto.Firstname);
      Equal(customer1Dto.Lastname, actual1Dto.Lastname);
      Equal(customer1Dto.CompanyName, actual1Dto.CompanyName);
      Equal(customer1Dto.StatusInt, actual1Dto.StatusInt);
      Equal(customer1Dto.Email, actual1Dto.Email);
      Equal(customer1Dto.AddressDto, actual1Dto.AddressDto);
      
      Equal(customer2Dto.Id, actual2Dto.Id);
      Equal(customer2Dto.Firstname, actual2Dto.Firstname);
      Equal(customer2Dto.Lastname, actual2Dto.Lastname);
      Equal(customer2Dto.CompanyName, actual2Dto.CompanyName);
      Equal(customer2Dto.StatusInt, actual2Dto.StatusInt);
      Equal(customer2Dto.Email, actual2Dto.Email);
      Equal(customer2Dto.AddressDto, actual2Dto.AddressDto);
   }
   
   [Fact]
   public async Task SelectByNameAsync_loads_CompanyName_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer5 = customers[4];
      var customer5Dto = customer5.ToCustomerDto();

      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectByDisplayNameAsync("Conrad Consulting", ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDtos = result.Value.ToList();
      Single(actualDtos);
      var actual5Dto = actualDtos[0];
      
      Equal(customer5Dto.Id, actual5Dto.Id);
      Equal(customer5Dto.Firstname, actual5Dto.Firstname);
      Equal(customer5Dto.Lastname, actual5Dto.Lastname);
      Equal(customer5.CompanyName, actual5Dto.CompanyName);
      Equal(customer5Dto.StatusInt, actual5Dto.StatusInt);
      Equal(customer5Dto.Email, actual5Dto.Email);
      Equal(customer5Dto.AddressDto, actual5Dto.AddressDto); 
   }
   
   [Fact]
   public async Task SelectAll_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectAllAsync( ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value.ToList();
      Equal(6, actual.Count());
      NotNull(actual);
      Equals(customers, actual);
   
   }
}