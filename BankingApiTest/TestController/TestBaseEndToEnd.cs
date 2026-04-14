using BankingApiTest.TestInfrastructure;
namespace BankingApiTest.TestController;

/// Base class for end to end tests that need a fresh database per test.
/// Creates a new BankingApiFactory (and thus a new DB file) for every test instance.
/// xUnit creates a NEW instance of the test class per test method by default.
public abstract class TestBaseEndToEnd : IAsyncLifetime {
   protected TestBaseFactory Factory { get; private set; } = null!;
   protected HttpClient Client { get; private set; } = null!;

   // Teaching mode:
   // - Keep DB files so students can inspect them in Rider after a debug session.
   // CI mode:
   // - Set to true to delete DB files after each test.
   protected virtual bool DeleteDatabaseOnDispose => false;

   // Create a unique DB file per test (FileUnique).
   // If you want a stable path per test name, keep FileUnique and use the test method name in DatabaseName.
   protected virtual DbMode DbMode => DbMode.FileUnique;

   // Base name for DB files (timestamp will be appended for FileUnique).
   // Override to include the test name if you want even nicer traceability.
   protected virtual string DatabaseName => "BankingApiTest";

   public async ValueTask InitializeAsync() {
      Factory = new TestBaseFactory(
         dbMode: DbMode,
         databaseName: DatabaseName,
         applyMigrations: true,
         enableSensitiveDataLogging: true,
         deleteDatabaseOnDispose: DeleteDatabaseOnDispose
      );

      await Factory.InitializeAsync();
      Client = Factory.CreateClient();

      // Optional: log the DB path so students can open it in Rider quickly.
      Console.WriteLine($"---> Test DB Path: {Factory.DatabasePath}");
   }

   public async ValueTask DisposeAsync() {
      Client.Dispose();
      await Factory.DisposeAsync();
   }
}

/*
DEUTSCHER DIDAKTIK-BLOCK (Vorlesung / Lernziele)

Warum "pro Test"?
- Jeder Test startet mit einer frischen, leeren Datenbank.
- Das macht Tests unabhängig (keine versteckten Abhängigkeiten / Reihenfolgeeffekte).
- In Debug-Sessions können Studierende den DB-Zustand eines einzelnen Tests nachvollziehen
  (ohne Altlasten von vorherigen Tests).

Warum FileUnique?
- Es entsteht pro Test eine eigene SQLite-Datei (einzigartig).
- Rider kann diese Datei öffnen und Tabellen/Views anzeigen.
- Für CI/Parallelität ist das robust, weil Tests sich nicht gegenseitig beeinflussen.

Wichtige Lernpunkte:
1) xUnit Lebenszyklus
   - Standardmäßig erzeugt xUnit pro Testmethode eine neue Instanz der Testklasse.
   - Dadurch kann die Base-Class pro Test eine neue Factory/DB erzeugen.

2) Realistische DB-Struktur mit Migrationen
   - Migrate() statt EnsureCreated(), damit Views und SQL aus Migrationen existieren.

3) Nachvollziehbarkeit im Unterricht
   - DeleteDatabaseOnDispose=false behält die DB-Datei nach dem Testlauf.
   - Studierende können im Rider Database Viewer nach dem Debug weiter inspizieren.

Lernziele:
- Studierende verstehen Test-Isolation und warum "fresh DB per test" wichtig ist.
- Studierende können den DB-Zustand als Debug-Artefakt nutzen (Tabellen/Views/Rows).
- Studierende verstehen den Zusammenhang DI (Program.cs) + Test-Overrides (Factory).
*/

