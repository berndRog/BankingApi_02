using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest.TestInfrastructure;

// Base class for integration tests that need a fresh <see cref="TestCompositionRoot"/>
// and therefore a fresh SQLite database per test method.
// xUnit creates a new instance of the test class for every test method.
public abstract class TestBaseIntegration : IAsyncLifetime {

   protected DbMode DbMode { get; set; } = DbMode.InMemory;
   protected string DbName { get; set; } = "BankingTest";
   protected bool SensitiveDataLogging { get; set; } = false; 
   
   protected TestCompositionRoot Root { get; private set; } = null!;

   // ctor
   protected TestBaseIntegration() { }
   
   public async ValueTask InitializeAsync() {
      Root = new TestCompositionRoot(DbName, DbMode, SensitiveDataLogging);
      await Root.InitializeAsync();
   }

   public async ValueTask DisposeAsync() {
      await Root.DisposeAsync();
   }
}

