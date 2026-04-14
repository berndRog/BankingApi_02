using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

// Read model interface for querying customer data.
// Used by the application layer and API controllers to retrieve
// customer information without exposing the domain model.
// Returns DTOs because this port belongs to the query side.
public interface ICustomerReadModel {

   // Returns the currently authenticated customer
   Task<Result<CustomerDto>> FindMeAsync(
      CancellationToken ct = default
   );

   // Find customer by technical identifier
   Task<Result<CustomerDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Find customer by email address
   Task<Result<CustomerDto>> FindByEmailAsync(
      string email,
      CancellationToken ct = default
   );

   // Return all customers
   Task<Result<IEnumerable<CustomerDto>>> SelectAllAsync(
      CancellationToken ct
   );
   
   // Load all customers with SQL like displayName
   Task<Result<IEnumerable<CustomerDto>>> SelectByDisplayNameAsync(
      string displayName,
      CancellationToken ct = default
   );

   // Optional filtering / paging query
   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );
}

/*
Didaktik
--------

Dieses Interface beschreibt ein ReadModel im Customers-Bounded-Context.

Ein ReadModel wird für Abfragen (Queries) verwendet und liefert DTOs
anstatt Domain-Objekten zurück.

Die Application Layer greift über dieses Interface auf
Leseoperationen zu, während die konkrete Implementierung
in der Infrastructure liegt.

Ein wichtiger Unterschied zu Repositories:

Repository
- arbeitet mit Aggregates
- wird für Commands und Domainlogik verwendet

ReadModel
- arbeitet mit DTOs
- wird für Anzeige, Listen und Suchabfragen genutzt

Dadurch kann die Lese-Seite unabhängig vom Domain Model
optimiert werden.


Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- Trennung von Command- und Query-Seite (CQRS light)
- Einsatz von Ports zur Entkopplung der Infrastruktur
- Verwendung von DTOs für effiziente Leseoperationen
*/