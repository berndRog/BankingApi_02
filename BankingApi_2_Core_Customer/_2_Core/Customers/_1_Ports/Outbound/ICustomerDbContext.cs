using BankingApi._2_Core.Customers._3_Domain.Entities;

namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

// Database context abstraction used by the Customers module.
// Provides minimal persistence access required by repositories.
// The concrete implementation typically wraps an EF Core DbContext.
public interface ICustomerDbContext {

   // Query access to Customer aggregates
   IQueryable<Customer> Customers { get; }

   // Add a new entity to the persistence context
   void Add(Customer customer);
   void AddRange(IEnumerable<Customer> customers);

   // Remove an entity from the persistence context
   void Update(Customer customer);
}

/*
Didaktik
--------

Dieses Interface abstrahiert den Datenbankzugriff für
den Customers-Bounded-Context.

Es handelt sich nicht um ein Repository, sondern um eine
technische Abstraktion über den konkreten ORM-Context
(z.B. EF Core DbContext).

Repositories verwenden dieses Interface, um auf die
Persistenz zuzugreifen.

Dadurch entsteht eine saubere Trennung:

Application / Domain
→ arbeitet mit Repository-Interfaces

Infrastructure
→ implementiert DbContext und Repositories

Der Vorteil dieser Abstraktion:

- bessere Testbarkeit
- Entkopplung von EF Core
- klar definierte Datenzugriffsgrenze


Lernziele
---------

- Unterschied zwischen Repository und DbContext verstehen
- Rolle eines Persistence Ports in Clean Architecture
- Entkopplung der Domain von konkreten ORM-Technologien
- Verbesserung der Testbarkeit durch Abstraktion
*/