using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
   public AppDbContext CreateDbContext(string[] args) {
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false)
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();

      var connectionString = configuration.GetConnectionString("BankingApiDb");
      Console.WriteLine("---> Using SQLite connection string: " + connectionString);

      var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
      // Passen Sie den Connection String an Ihre Umgebung an
      optionsBuilder.UseSqlite(connectionString);
      // Oder für SQL Server:
      // optionsBuilder.UseSqlServer("Server=localhost;Database=banking_dev;Trusted_Connection=True;");

      return new AppDbContext(optionsBuilder.Options);
   }
}