using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;

namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

// Repository port for accessing Customer aggregates.
// Defines the persistence operations required by the application layer.
// Implementations live in the Infrastructure layer (e.g. EF Core).
public interface ICustomerRepository {

   // Load a customer aggregate by its identifier
   Task<Customer?> FindByIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // Load a customer using the identity subject (IdP reference)
   Task<Customer?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct = default
   );

   // Load a customer by email value object
   Task<Customer?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct = default
   );

   // Load all customers with SQL like displayName
   Task<IReadOnlyList<Customer>> SelectByDisplayNameAsync(
      string displayName,
      CancellationToken ct = default
   );
   
   // Check if the customer exists and is currently active
   Task<bool> ExistsActiveAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // Select all customers
   Task<IReadOnlyList<Customer>> SelectAllAsync(
      CancellationToken ct = default
   );
   
   // Add a new customer aggregate to the repository
   void Add(Customer customer);
   void AddRange(IEnumerable<Customer> customers);
   
   // Update a customer aggregate in the repository
   void Update(Customer customer);
}

/*
Didaktik
--------

Dieses Interface beschreibt ein Repository im Sinne von
Domain-Driven Design.

Ein Repository stellt aus Sicht der Domain eine Sammlung
von Aggregaten dar.

Der Zugriff erfolgt ausschließlich über fachlich sinnvolle
Methoden, z.B.:

- FindById
- FindByEmail
- ExistsActive

Wichtig ist, dass das Repository mit Domain-Objekten arbeitet:

Repository
→ arbeitet mit Aggregates (Customer)

ReadModel
→ arbeitet mit DTOs (CustomerDto)

Die konkrete Implementierung des Repositories befindet sich
in der Infrastructure-Schicht (z.B. EF Core).


Lernziele
---------

- Rolle eines Repositories im Domain Model verstehen
- Unterschied zwischen Repository und ReadModel erkennen
- Trennung zwischen Domain und Persistenz
- Einsatz von Ports zur Entkopplung der Infrastruktur
*/