using System.Data.Common;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
namespace BankingApiTest.TestInfrastructure;

// Concrete test composition root for BankingDbContext
public sealed class TestCompositionRoot : TestCompositionRoot<AppDbContext> {

   public TestCompositionRoot(
      string dbName = "BankingTest",
      DbMode dbMode = DbMode.InMemory,
      bool sensitiveDataLogging = false
   ) {
      DatabaseName = dbName;
      DatabaseMode = dbMode;
      ApplyMigrations =  true;
      EnableSensitiveDataLoggingForDatabase = sensitiveDataLogging;
   }

   // Create the concrete BankingDbContext
   protected override AppDbContext CreateDbContext(DbContextOptions<AppDbContext> options) {
      return new AppDbContext(options);
   }

   // Register all project services needed for tests
   protected override void AddProjectServices(IServiceCollection services) {
      services.AddTestModules(
         dbConnection: DbConnection,
         enableSensitiveDataLogging: EnableSensitiveDataLoggingForDatabase
      );
   }
}

// Base composition root for integration tests
// Creates database, DI container, and shared infrastructure
public abstract class TestCompositionRoot<TDbContext> : IAsyncLifetime
   where TDbContext : DbContext {

   // Logical name of the test database
   public string DatabaseName { get; protected set; } = "DatabaseTest";

   // Database mode used for test execution
   public DbMode DatabaseMode{ get; protected set; } = DbMode.InMemory;

   // Apply EF Core migrations on startup
   public  bool ApplyMigrations { get; protected set; } =  true;

   // Enable EF sensitive data logging for tests
   public bool EnableSensitiveDataLoggingForDatabase { get; protected set; }

   // File path of the created test database
   public string DbPath { get; private set; } = string.Empty;
   // Shared database connection used by tests
   public DbConnection DbConnection { get; private set; } = default!;
   
   // DbContext used during initialization and seeding
   internal TDbContext InitDbContext { get; private set; } = default!;

   // Default service provider used by most tests
   public ServiceProvider DefaultProvider { get; private set; } = default!;

   public IConfiguration Configuration { get; private set; } = default!;

   public async ValueTask InitializeAsync() {
      
      // load appsettingTest.json
      Configuration = TestConfiguration.Load();

      // Create database, connection, and initialization DbContext
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync<TDbContext>(
         createDbContext: CreateDbContext,
         mode: DatabaseMode,
         databaseName: DatabaseName,
         applyMigrations: ApplyMigrations,
         enableSensitiveDataLogging: EnableSensitiveDataLoggingForDatabase,
         ct: default
      );

      DbPath = dbPath;
      DbConnection = dbConnection;
      InitDbContext = dbContext;

      // Build the default DI container
      DefaultProvider = CreateProviderCore();
   }

   private void AddBaseServices(IServiceCollection services) {
      services.AddSingleton(Configuration);

      // Configure logging for test runs
      services.AddLogging(builder => {
         builder.ClearProviders();

         builder.AddConfiguration(Configuration.GetSection("Logging"));
         builder.AddSimpleConsole(o => {
            o.SingleLine = false;
            o.TimestampFormat = "HH:mm:ss ";
         });
      });

      // Register project-specific services
      AddProjectServices(services);
   }

   // Factory method for creating the concrete DbContext
   protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);

   // Register application and infrastructure services for tests
   protected abstract void AddProjectServices(IServiceCollection services);

   private ServiceProvider CreateProviderCore(
      Action<IServiceCollection>? overrides = null
   ) {
      var services = new ServiceCollection();

      // Register base services
      AddBaseServices(services);

      // Apply optional test overrides
      overrides?.Invoke(services);

      return services.BuildServiceProvider();
   }

   // Return the default provider or build a custom one with overrides
   public ServiceProvider BuildServiceProvider(
      Action<IServiceCollection>? overrides = null
   ) {
      return overrides is null
         ? DefaultProvider
         : CreateProviderCore(overrides);
   }

   // Create a scope from the default provider
   public IServiceScope CreateDefaultScope() {
      return DefaultProvider.CreateScope();
   }

   // Build a custom provider for a specific test setup
   public ServiceProvider CreateCustomProvider(
      Action<IServiceCollection> overrides
   ) {
      return CreateProviderCore(overrides);
   }

   // Replace a singleton registration in tests
   public static void ReplaceSingleton<TService>(
      IServiceCollection services,
      TService implementation
   )
      where TService : class {
      services.RemoveAll<TService>();
      services.AddSingleton(implementation);
   }

   // Replace a scoped registration in tests
   public static void ReplaceScoped<TService, TImplementation>(
      IServiceCollection services
   )
      where TService : class
      where TImplementation : class, TService {
      services.RemoveAll<TService>();
      services.AddScoped<TService, TImplementation>();
   }

   public async ValueTask DisposeAsync() {

      // Dispose the default DI container
      await DefaultProvider.DisposeAsync();

      // Dispose database resources
      await TestDatabase.DisposeAsync(
         mode: DatabaseMode,
         dbPath: DbPath,
         dbConnection: DbConnection,
         dbContext: InitDbContext,
         deleteDatabaseFile: false
      );
   }
}

internal static class TestConfiguration {
   private const string AppSettingsFileName = "appsettingsTest.json";

   internal static IConfigurationRoot Load() {
      return new ConfigurationBuilder()
         .SetBasePath(AppContext.BaseDirectory)
         .AddJsonFile(path: AppSettingsFileName, optional: false, reloadOnChange: false)
         .AddEnvironmentVariables()
         .Build();
   }
}


/*
Didaktik
--------

Diese Klasse bildet den zentralen Composition Root
für Integrationstests.

Hier wird die technische Testinfrastruktur aufgebaut:

- Testdatenbank
- DbContext
- Dependency Injection Container
- Logging
- Projektmodule

Dadurch bleiben einzelne Tests klein und konzentrieren
sich auf fachliches Verhalten.

Ein wichtiger Punkt ist die Möglichkeit, Registrierungen
für Tests gezielt zu ersetzen, z.B. durch Fakes oder Mocks.

Beispiele:

- FakeClock
- FakeIdentityGateway
- FakePaymentGateway

So kann die Testumgebung flexibel angepasst werden,
ohne den Produktionscode zu verändern.


Lernziele
---------

- Verständnis eines Composition Root für Tests
- Aufbau einer stabilen Infrastruktur für Integrationstests
- Einsatz von Dependency Injection im Testkontext
- Überschreiben von Registrierungen mit Test Doubles
- Trennung zwischen technischem Setup und fachlichem Testcode
*/
