using System.Data.Common;
using BankingApi;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest._3_Infrastructure._3_Security;
using BankingApiTest._3_Infrastructure._5_Utils;
using BankingApiTest.TestInfrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace BankingApiTest.TestController;

/// <summary>
/// Integration-test host for BankingApi.
/// Uses the real Program.cs DI setup and only replaces selected infrastructure services (e.g., the database).
/// </summary>
public sealed class TestBaseFactory : WebApplicationFactory<Program> {
   private readonly DbMode _dbMode;
   private readonly string _databaseName;
   private readonly bool _applyMigrations;
   private readonly bool _enableSensitiveDataLogging;
   private readonly bool _deleteDatabaseOnDispose;

   private string _dbPath = string.Empty;
   private DbConnection? _dbConnection;
   
   public string TestSubject { get; set; } = "11111111-a224-492b-bb8f-b4bac23d7c88";
   public string TestUsername { get; set; } = "j.doe@mail.local";
   public DateTimeOffset TestCreatedAt { get; set; } = DateTimeOffset.Parse("2025-01-01T00:00:00+01:00");
   public int TestAdminRights { get; set; }

   
   public TestBaseFactory(
      DbMode dbMode,
      string databaseName = "BankingApiTest",
      bool applyMigrations = true,
      bool enableSensitiveDataLogging = true,
      bool deleteDatabaseOnDispose = false
   ) {
      _dbMode = dbMode;
      _databaseName = databaseName;
      _applyMigrations = applyMigrations;
      _enableSensitiveDataLogging = enableSensitiveDataLogging;
      _deleteDatabaseOnDispose = deleteDatabaseOnDispose;
   }

   public async Task InitializeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         mode: _dbMode,
         databaseName: _databaseName,
         applyMigrations: _applyMigrations,
         enableSensitiveDataLogging: _enableSensitiveDataLogging
      );

      _dbPath = dbPath;
      _dbConnection = dbConnection;

      // Only for initialization. Do not keep scoped DbContext instances around.
      await dbContext.DisposeAsync();
   }

   public override async ValueTask DisposeAsync() {
      await TestDatabase.DisposeAsync(
         mode: _dbMode,
         dbPath: _dbPath,
         dbConnection: _dbConnection,
         dbContext: null,
         deleteDatabaseFile: _deleteDatabaseOnDispose
      );
      
      await base.DisposeAsync();
   }

   protected override void ConfigureWebHost(IWebHostBuilder builder) {
      builder.ConfigureAppConfiguration((_, config) => {
         config.AddJsonFile(
            path: Path.Combine(AppContext.BaseDirectory, "appsettingsTest.json"),
            optional: false,
            reloadOnChange: false
         );
      });
      
      builder.ConfigureServices(services => {
         if (_dbConnection is null)
            throw new InvalidOperationException("Factory not initialized. Did you call InitializeAsync()?");

         // 1) Remove all registrations that might exist from Program.cs
         services.RemoveAll<DbContextOptions<AppDbContext>>();
         services.RemoveAll<AppDbContext>();

         // Optional: if you use IDbContextFactory<BankingDbContext> anywhere
         services.RemoveAll<IDbContextFactory<AppDbContext>>();

         // 2) Re-register BankingDbContext using the test connection
         services.AddDbContext<AppDbContext>(options => {
            options.UseSqlite(_dbConnection);
            if (_enableSensitiveDataLogging) options.EnableSensitiveDataLogging();
         });

         // 3) Replace UnitOfWork
         services.RemoveAll<IUnitOfWork>();
         services.AddScoped<IUnitOfWork, UnitOfWork>();
         
         // replace more infrastructure for tests here (Clock, IdentityGateway)
         services.RemoveAll(typeof(IClock));
         services.AddSingleton<IClock>(new FakeClock(TestCreatedAt));
         
         services.RemoveAll(typeof(IIdentityGateway));
         services.AddScoped<IIdentityGateway>(_ =>
            new FakeIdentityGateway(TestSubject, TestUsername, TestCreatedAt, TestAdminRights));

          
         // ---- Fake auth for tests ----
         // Register test auth scheme (do NOT try to register "Bearer")
         services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
               TestAuthHandler.SchemeName, _ => { });

         // Force defaults LAST (this is the important bit for [Authorize])
         services.PostConfigureAll<AuthenticationOptions>(o =>
         {
            o.DefaultScheme = TestAuthHandler.SchemeName;
            o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
            o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
         });

         // Important: ensures authorization sees an authenticated user
         services.AddAuthorization();

         
      });
   }

   public string DatabasePath => _dbPath;

   public IServiceScope CreateScope() => Services.CreateScope();

   public async Task WithScopeAsync(Func<IServiceProvider, Task> action) {
      using var scope = CreateScope();
      await action(scope.ServiceProvider);
   }
}
