// using System.Data.Common;
// using BankingApi._2_Modules.AccountsTransfers._1_Ports.Inbound;
// using BankingApi._2_Modules.AccountsTransfers._1_Ports.Outbound;
// using BankingApi._2_Modules.AccountsTransfers._4_Infrastructure.Adapters;
// using BankingApi._2_Modules.AccountsTransfers._4_Infrastructure.Repositories;
// using BankingApi._2_Modules.Customers._1_Ports.Outbound;
// using BankingApi._2_Modules.Customers._2_Application.Dtos;
// using BankingApi._2_Modules.Customers._2_Application.UseCases;
// using BankingApi._2_Modules.Customers._3_Domain.Aggregates;
// using BankingApi._2_Modules.Customers._3_Domain.Enum;
// using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
// using BankingApi._3_Infrastructure.Database;
// using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
// using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
// using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
// using Microsoft.EntityFrameworkCore;
// namespace BankingApiTest.Modules.Customers.Infrastructure;
//
// [Collection("Sequential")]
// public sealed class EmployeesUcActivate_Reject_DeactivateIntT : TestBase, IAsyncLifetime {
//    
//    #region Test Setup
//    private string? _dbPath;
//    private DbConnection? _dbConnection;
//    private DbContext? _dbContext;
//    private Boolean _isInMemory = false;
//
//    private TestSeed _seed = null!;
//    private IClock _clock = null!;
//
//    private ICustomerRepository _repository = null!;
//    private IUnitOfWork _unitOfWork = null!;
//    
//    private IIdentityGateway _identityGateway = null!;
//    private CustomerUcCreateProvision _ownerUcCreateProvisioned = null!;
//    private CustomerUcUpdateProfile _ownerUcUpdateProfile = null!;
//    
//    private IIdentityGateway _identityGatewayAdmin = null!;
//    private CustomerUcActivate _ucActivate = null!;
//    private CustomerUcReject _ucReject = null!;
//    private CustomerUcDeactivate _ucDeactivate = null!;
//    
//    private IAccountsContract _accountsContract = null!;
//    private IAccountsRepository _accountsRepository = null!;
//    
//    
//    
//    private Guid _customerId;
//    private string _id = default!;
//    private string _subject = default!;
//    private string _username = default!;
//    private DateTimeOffset _createdAt;
//    private int _adminRights;
//    private CancellationToken _ct = default!;
//    
//    public async Task InitializeAsync() {
//       _ct = CancellationToken.None;
//       _clock = new FakeClock(new DateTime(2025, 01, 01));
//       _seed = new TestSeed();
//
//       // create a real database for testing,
//       // as in-memory databases do not support all features (e.g. transactions, relational constraints)
//       var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
//          useInMemory: _isInMemory, projectName: "BankingApiTest", _ct);
//       _dbPath = dbPath;
//       _dbConnection = dbConnection;
//       _dbContext = dbContext;
//       var bankingDbContext = _dbContext   as BankingDbContext ?? 
//          throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");
//
//       _repository = new EmployeesRepositoryEf(bankingDbContext);
//       _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
//
//       _accountsRepository = new AccountsRepositoryEf(bankingDbContext);
//       _accountsContract = new AccountsContract(
//          _accountsRepository, 
//          _unitOfWork, 
//          _clock, 
//          CreateLogger<AccountsContract>()
//       );
//       
//       // Test Customer
//       _id = _seed.Customer5.Id.ToString();
//       _customerId = _seed.Customer5.Id;
//       _subject = _seed.Customer5.Subject;
//       _username = _seed.Customer5.Email.Value;
//       _createdAt = _seed.Customer5.CreatedAt;
//       
//       // Default gateway
//       _identityGateway = new FakeIdentity(clock: _clock, subject: _subject, 
//          username: _username, createdAt: _createdAt, adminRights: _adminRights);
//       
//       // create provioned
//       _ownerUcCreateProvisioned = new CustomerUcCreateProvision(_identityGateway, _repository, _unitOfWork, 
//          _clock, CreateLogger<CustomerUcCreateProvision>());
//       
//       // upsert provisioned
//       _ownerUcUpdateProfile = new CustomerUcUpdateProfile(_identityGateway, _repository,
//          _unitOfWork, _clock, CreateLogger<CustomerUcUpdateProfile>()
//       );
//       
//       // simulate a login in Admin
//       _adminRights = 255;
//       // Default gateway
//       _identityGatewayAdmin = new FakeIdentity(clock: _clock, subject: _subject, 
//          username: _username, createdAt: _createdAt, adminRights: _adminRights);
//       
//       // activate
//       _ucActivate = new CustomerUcActivate(_identityGatewayAdmin, _repository, _accountsContract,
//          _unitOfWork, _clock, TestLogger.Create<CustomerUcActivate>(true));
//       
//       // reject
//       _ucReject = new CustomerUcReject(_identityGatewayAdmin, _repository,
//          _unitOfWork, _clock, TestLogger.Create<CustomerUcReject>(true));
//       
//       // deactivate use cases
//       _ucDeactivate = new CustomerUcDeactivate(_identityGatewayAdmin, _repository,
//          _unitOfWork, _clock, TestLogger.Create<CustomerUcDeactivate>(true));
//       
//       // provision owner use case
//       var resultProvisioned = 
//          await _ownerUcCreateProvisioned.ExecuteAsync(_id, CancellationToken.None);
//       True(resultProvisioned.IsSuccess);
//       _customerId = resultProvisioned.Value.Id;
//       
//       // upsert profile use case
//       var ownerProfileDto = new CustomerDto(
//          Id: _seed.Customer5.Id,
//          Firstname: _seed.Customer5.Firstname,
//          Lastname: _seed.Customer5.Lastname,
//          CompanyName: _seed.Customer5.CompanyName,
//          Email: "c.conrad@mail.local",
//          Status: (int) _seed.Customer5.Status,
//          CreatedAt: _seed.Customer5.CreatedAt,
//          DeactivatedAt: _seed.Customer5.DeactivatedAt,
//          Street:  _seed.Customer5.Address?.Street,
//          PostalCode: _seed.Customer5.Address?.PostalCode,
//          City: _seed.Customer5.Address?.City,
//          Country: _seed.Customer5.Address?.Country
//       );
//       var resultUpsert = await _ownerUcUpdateProfile.ExecuteAsync(ownerProfileDto, _ct); 
//       True(resultUpsert.IsSuccess);
//
//    }
//
//    public async Task DisposeAsync() {
//       var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
//          _isInMemory, _dbPath, _dbConnection, _dbContext);
//       _dbPath = dbPath;
//       _dbConnection = dbConnection;
//       _dbContext = dbContext;
//    }
//    #endregion
//
//    
//    [Fact]
//    public async Task ActivateAsync_returns_success() {
//       // Arrange
//       var resultCreate = await _ownerUcCreateProvisioned.ExecuteAsync(_id, _ct);
//       True(resultCreate.IsSuccess);
//       
//       // Act
//       var resultActivate = await _ucActivate.ExecuteAsync(_customerId, _customerId, null, _ct);   
//       _dbContext!.ChangeTracker.Clear();
//
//       // Assert
//       True(resultActivate.IsSuccess);
//       var actual = await _repository.FindByIdAsync(_customerId,  _ct);
//       NotNull(actual);
//       Equal(_customerId, actual.Id);
//       Equal(CustomerStatus.Active, actual.Status);
//       
//    }
//    /*
//    [Fact]
//    public async Task UpdateProfileAsync_returns_success() {
//       // Arrange
//       _ucCreateProvisioned = new CustomerUcCreateProvision(_identityGateway, _repository,
//          _unitOfWork, _clock, TestLogger.Create<CustomerUcCreateProvision>(true));
//       var subject = _identityGateway.Subject;
//       var email = _identityGateway.Username.ToLowerInvariant(); // email is derived from username and normalized to lower case
//       var createdAt = _identityGateway.CreatedAt;
//       var id = "50000000-0000-0000-0000-000000000000";
//       
//       // create provisioned owner first
//       var result = await _ucCreateProvisioned.ExecuteAsync(id: id, ct: _ct);   
//       _dbContext.ChangeTracker.Clear();
//       
//       // owner profile 
//       var owner = _seed.Customer5; 
//       var ownerProfileDto = new OwnerProfileDto(
//          Firstname: owner.Firstname,
//          Lastname: owner.Lastname,
//          CompanyName: owner.CompanyName,
//          Email: email, // same email, should not cause uniqueness error
//          Street: owner.Address?.Street,
//          PostalCode: owner.Address?.PostalCode,
//          City: owner.Address?.City,
//          Country: owner.Address?.Country
//       );
//       
//       // Act: update profile 
//       _ucUpsertProfile = new OwnerUcUpsertProfile(
//          _identityGateway,
//          _repository,
//          _unitOfWork,
//          _clock,
//          TestLogger.Create<OwnerUcUpsertProfile>(true)
//       );
//       var resultUpsert = await _ucUpsertProfile.ExecuteAsync(ownerProfileDto, _ct);
//       _dbContext.ChangeTracker.Clear();
//       
//       // Assert
//       True(resultUpsert.IsSuccess);
//       var actual = await _repository.FindByIdAsync(owner.Id, noTracking: true, _ct);
//       NotNull(actual);
//       Equal(Guid.Parse(id), actual.Id);
//       Equal(email, actual.Email);
//       Equal(subject, actual.Subject);
//       NotNull(actual.Address);
//       Equal(owner.Address?.Street, actual.Address!.Street);
//       Equal(owner.Address?.PostalCode, actual.Address!.PostalCode);
//       Equal(owner.Address?.City, actual.Address!.City);
//       Equal(owner.Address?.Country, actual.Address!.Country);
//       Equal(owner.CreatedAt, actual.CreatedAt);
//
//    }
//    */
// }