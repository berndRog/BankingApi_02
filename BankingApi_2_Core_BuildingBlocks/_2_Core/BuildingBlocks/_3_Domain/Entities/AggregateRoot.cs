namespace BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;

// Base class for all aggregate roots in the domain model.
//
// Responsibilities:
// - Inherits identity semantics from Entity.
// - Manages audit timestamps (CreatedAt, UpdatedAt).
// - Ensures time abstraction via IClock (testability).
public abstract class AggregateRoot : Entity {

   // Timestamp when the aggregate was created (Should only be set once).
   public DateTimeOffset CreatedAt { get; protected set; }

   // Timestamp of the last modification of the aggregate (Updated on every state change).
   public DateTimeOffset UpdatedAt { get; protected set; }

   // Ctor injection of time provider.
   // - No direct dependency on system clock.
   // - Proper initialization of audit fields.
   protected AggregateRoot() { }
   
   // Explicitly sets the creation timestamp.
   // Guard: CreatedAt must not be default.
   protected void Initialize(DateTimeOffset createdAt) {
      if (createdAt == default)
         throw new ArgumentException("createdAt must be set.", nameof(createdAt));

      CreatedAt = createdAt;
      UpdatedAt = createdAt;
   }
   
   // Updates the modification timestamp.
   protected void Touch(DateTimeOffset updatedAt) =>
      UpdatedAt = updatedAt;

   // OPTIONAL EXTENSIONS (not implemented – shown for architectural evolution)
   // ------------------------------------------------------------------------
   // 1. Domain Events Support
   //
   // private readonly List<IDomainEvent> _domainEvents = new();
   //
   // public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
   //
   // protected void AddDomainEvent(IDomainEvent domainEvent){
   //     _domainEvents.Add(domainEvent);
   // }
   // public void ClearDomainEvents(){
   //     _domainEvents.Clear();
   // }
   // Purpose:
   // - Enables event-driven architecture.
   // - Allows Outbox pattern integration.
   // - Keeps event recording inside aggregate boundaries.

   // 2. Soft Delete Support
   //
   // public bool IsDeleted { get; protected set; }
   //
   // protected void MarkAsDeleted() {
   //     IsDeleted = true;
   //     Touch();
   // }
   // Purpose:
   // - Enables logical deletion.
   // - Avoids physical removal for audit compliance.

   // 3. Versioning / Concurrency Control
   //
   // public int Version { get; protected set; }
   //
   // protected void IncrementVersion(){
   //     Version++;
   // }
   //
   // Purpose:
   // - Optimistic concurrency control.
   // - Required for distributed systems.
}
/*
================================================================================
DIDAKTIK UND LERNZIELE (Aggregate Root – Minimalversion)
================================================================================

1. Unterschied zwischen Entity und AggregateRoot verstehen
   - Entity: Identitätskonzept.
   - AggregateRoot: Einstiegspunkt eines Konsistenzbereichs (Consistency Boundary).

2. Zeit als Abhängigkeit modellieren
   - Kein direkter Zugriff auf DateTimeOffset.UtcNow.
   - Verwendung eines IClock abstrahiert Infrastruktur.
   - Ermöglicht deterministische Unit Tests.

3. Audit-Felder als Domänenverantwortung
   - CreatedAt und UpdatedAt gehören zum Lebenszyklus des Aggregats.
   - Touch() verdeutlicht explizite Zustandsänderung.

4. Konsistenzgrenzen begreifen
   - Nur AggregateRoot darf von außen referenziert werden.
   - Innere Entities sind nur über das Root manipulierbar.

5. Vorbereitung auf fortgeschrittene Architekturkonzepte
   - Domain Events (Event-getriebene Architektur)
   - Outbox Pattern
   - Optimistic Concurrency
   - Soft Deletes
   - Versionierung

6. Minimalismus als Architekturprinzip
   - Keine Repository-Logik
   - Keine Persistenz
   - Keine Framework-Abhängigkeiten
   - Nur fachliche Verantwortung

Zentrales Lernziel:
Studierende sollen verstehen, dass ein Aggregate Root
nicht nur eine Entity mit zusätzlichen Feldern ist,
sondern eine fachliche Konsistenz- und Transaktionsgrenze
im Sinne von Domain-Driven Design darstellt.

================================================================================
*/