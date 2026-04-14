using System.Data.Common;
using System.Reflection;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace BankingApiTest.TestInfrastructure;

// Helper for creating and disposing test databases.
// Supports SQLite in-memory and file-based modes.
public static class TestDatabase {

   // Create a test database for the given DbContext type
   public static Task<(string dbPath, DbConnection dbConnection, TDbContext dbContext)> CreateAsync<TDbContext>(
      Func<DbContextOptions<TDbContext>, TDbContext> createDbContext,
      DbMode mode,
      string databaseName,
      bool applyMigrations,
      bool enableSensitiveDataLogging,
      CancellationToken ct
   )
      where TDbContext : DbContext {

      ArgumentNullException.ThrowIfNull(createDbContext);

      databaseName = (databaseName ?? string.Empty).Trim();
      if (string.IsNullOrWhiteSpace(databaseName))
         throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

      return mode switch {
         DbMode.InMemory => CreateInMemoryAsync<TDbContext>(
            createDbContext: createDbContext,
            applyMigrations: applyMigrations,
            enableSensitiveDataLogging: enableSensitiveDataLogging,
            ct: ct
         ),

         DbMode.FilePersistent => CreateFileAsync<TDbContext>(
            createDbContext: createDbContext,
            stableFileName: $"{databaseName}.db",
            uniquePerRun: false,
            applyMigrations: applyMigrations,
            enableSensitiveDataLogging: enableSensitiveDataLogging,
            ct: ct
         ),

         DbMode.FileUnique => CreateFileAsync<TDbContext>(
            createDbContext: createDbContext,
            stableFileName: $"{databaseName}_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss_ffffff}.db",//_{Guid.NewGuid():N}.db",
            uniquePerRun: true,
            applyMigrations: applyMigrations,
            enableSensitiveDataLogging: enableSensitiveDataLogging,
            ct: ct
         ),

         _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown DbMode.")
      };
   }

   // Create a BankingDbContext test database
   public static async Task<(string dbPath, DbConnection dbConnection, DbContext dbContext)> CreateAsync(
      DbMode mode = DbMode.FilePersistent,
      string databaseName = "BankingApiTest",
      bool applyMigrations = true,
      bool enableSensitiveDataLogging = true,
      CancellationToken ct = default
   ) {
      var (dbPath, dbConnection, dbContext) = await CreateAsync<AppDbContext>(
         createDbContext: options => new AppDbContext(options),
         mode: mode,
         databaseName: databaseName,
         applyMigrations: applyMigrations,
         enableSensitiveDataLogging: enableSensitiveDataLogging,
         ct: ct
      );

      return (dbPath, dbConnection, dbContext);
   }

   // Dispose database resources
   // Optionally delete the database file
   public static async Task DisposeAsync(
      DbMode mode,
      string? dbPath,
      DbConnection? dbConnection,
      DbContext? dbContext,
      bool deleteDatabaseFile = false
   ) {
      // Dispose DbContext first
      if (dbContext is not null)
         await dbContext.DisposeAsync();

      // Dispose connection
      if (dbConnection is SqliteConnection sqliteConnection) {
         await sqliteConnection.CloseAsync();
         await sqliteConnection.DisposeAsync();
      }
      else if (dbConnection is not null) {
         await dbConnection.DisposeAsync();
      }

      // Delete file-based database if requested
      if (mode != DbMode.InMemory && deleteDatabaseFile && !string.IsNullOrWhiteSpace(dbPath))
         DeleteDatabaseFiles(dbPath!);
   }

   // Create an in-memory SQLite database
   private static async Task<(string dbPath, DbConnection dbConnection, TDbContext dbContext)> CreateInMemoryAsync<TDbContext>(
      Func<DbContextOptions<TDbContext>, TDbContext> createDbContext,
      bool applyMigrations,
      bool enableSensitiveDataLogging,
      CancellationToken ct
   )
      where TDbContext : DbContext {

      // Keep connection open for in-memory SQLite
      var connection = new SqliteConnection("Data Source=:memory:");
      await connection.OpenAsync(ct);

      // Apply SQLite settings
      await ApplySqlitePragmasAsync(connection, ct);

      var options = BuildOptions<TDbContext>(connection, enableSensitiveDataLogging);
      var dbContext = createDbContext(options);

      await InitializeSchemaAsync(dbContext, applyMigrations, ct);

      return (string.Empty, connection, dbContext);
   }

   // Create a file-based SQLite database
   private static async Task<(string dbPath, DbConnection dbConnection, TDbContext dbContext)> CreateFileAsync<TDbContext>(
      Func<DbContextOptions<TDbContext>, TDbContext> createDbContext,
      string stableFileName,
      bool uniquePerRun,
      bool applyMigrations,
      bool enableSensitiveDataLogging,
      CancellationToken ct
   )
      where TDbContext : DbContext {

      // Store database file in the test project directory
      var dbDir = FindTestProjectRoot();
      var dbPath = Path.Combine(dbDir, stableFileName);

      // Start with a clean database file
      DeleteDatabaseFiles(dbPath);

      var connectionString = $"Data Source={dbPath}";
      var connection = new SqliteConnection(connectionString);
      await connection.OpenAsync(ct);

      await ApplySqlitePragmasAsync(connection, ct);

      // Print database path for debugging
      Console.WriteLine($"---> Using SQLite test DB ({(uniquePerRun ? "unique" : "persistent")}): {dbPath}");

      var options = BuildOptions<TDbContext>(connection, enableSensitiveDataLogging);
      var dbContext = createDbContext(options);

      await InitializeSchemaAsync(dbContext, applyMigrations, ct);

      return (dbPath, connection, dbContext);
   }

   // Create schema using migrations or EnsureCreated
   private static async Task InitializeSchemaAsync<TDbContext>(
      TDbContext dbContext,
      bool applyMigrations,
      CancellationToken ct
   )
      where TDbContext : DbContext {

      var hasMigrations = applyMigrations &&
                          dbContext.Database.GetService<IMigrationsAssembly>().Migrations.Any();

      if (hasMigrations)
         await dbContext.Database.MigrateAsync(ct);
      else
         await dbContext.Database.EnsureCreatedAsync(ct);
   }

   // Build DbContextOptions for SQLite
   private static DbContextOptions<TDbContext> BuildOptions<TDbContext>(
      DbConnection connection,
      bool enableSensitiveDataLogging
   )
      where TDbContext : DbContext {

      var builder = new DbContextOptionsBuilder<TDbContext>()
         .UseSqlite(connection);

      if (enableSensitiveDataLogging)
         builder.EnableSensitiveDataLogging();

      return builder.Options;
   }

   // Apply SQLite settings helpful for tests and debugging
   private static async Task ApplySqlitePragmasAsync(SqliteConnection connection, CancellationToken ct) {
      await using var cmd = connection.CreateCommand();
      cmd.CommandText = """
                        PRAGMA journal_mode = DELETE;
                        PRAGMA busy_timeout = 5000;
                        PRAGMA foreign_keys = ON;
                        """;
      await cmd.ExecuteNonQueryAsync(ct);
   }

   // Delete database file and sidecar files
   private static void DeleteDatabaseFiles(string dbPath) {

      // Retry because file locking can happen on some systems
      for (int i = 0; i < 5; i++) {
         try {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists($"{dbPath}-wal")) File.Delete($"{dbPath}-wal");
            if (File.Exists($"{dbPath}-shm")) File.Delete($"{dbPath}-shm");
            return;
         }
         catch (IOException) {
            if (i == 4) throw;
            Thread.Sleep(150);
         }
      }
   }

   // Find the test project directory
   private static string FindTestProjectRoot() {

      // Usually the assembly name matches the test project name
      var projectName =
         Assembly.GetExecutingAssembly().GetName().Name
         ?? throw new InvalidOperationException("Could not determine test project name.");

      // Walk up from bin/Debug/... until the csproj is found
      var dir = new DirectoryInfo(AppContext.BaseDirectory);

      while (dir is not null) {
         if (dir.GetFiles($"{projectName}.csproj").Any())
            return dir.FullName;

         dir = dir.Parent;
      }

      throw new InvalidOperationException($"Could not find test project root for '{projectName}'.");
   }
}

// Defines how the test database is created
public enum DbMode {

   // SQLite in-memory database
   // Exists only while the connection is open
   InMemory,

   // Stable file name reused across runs
   FilePersistent,

   // Unique file name per run
   FileUnique
}

/*
Didaktik
--------

Dieser Helper kapselt die Erzeugung und Entsorgung
einer SQLite-Testdatenbank.

Er unterstützt zwei grundlegende Varianten:

- InMemory
- File-based

Die file-basierte Variante ist besonders nützlich
für die Lehre, weil Studierende die Datenbankdatei
im Projekt sehen und in Rider öffnen können.

Ein wichtiger Punkt ist die Initialisierung des Schemas:

- Migrate()
  → führt Migrationen aus
  → erstellt auch Views und SQL aus Migrationen

- EnsureCreated()
  → erstellt nur das Grundschema aus dem EF-Modell
  → führt keine Migrationen aus

Dadurch eignet sich dieser Helper gut für realistische
Integrationstests und für die Analyse des Datenbankzustands
während des Debuggens.


Lernziele
---------

- Unterschied zwischen EnsureCreated und Migrate verstehen
- SQLite in-memory und file-based vergleichen
- testbare Infrastruktur für Integrationstests aufbauen
- Datenbankdateien für Debugging und Lehre nutzbar machen
- Ressourcen sauber erzeugen und entsorgen
*/