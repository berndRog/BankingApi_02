namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
// Unit of Work abstraction for committing changes to the database.
// Coordinates repositories and ensures that multiple changes
// are saved in a single transaction.
public interface IUnitOfWork {

   // Persist all tracked changes to the database
   // Returns the number of affected rows/entities
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 

   // Clears the ORM change tracker
   // Useful in tests or after manual state corrections
   void ClearChangeTracker();

   // Writes the current change tracker state to logs
   // Helpful for debugging persistence behavior
   void LogChangeTracker(string text);
}

/*
Didaktik
--------

Das UnitOfWork kapselt das Speichern von Änderungen
im Persistence Layer.

Repositories laden und verändern Aggregate, aber
sie speichern diese Änderungen nicht direkt.

Stattdessen werden alle Änderungen gesammelt und
anschließend gemeinsam gespeichert:

Repository
   → lädt Aggregate
   → verändert Aggregate

UnitOfWork
   → speichert alle Änderungen in einer Transaktion

Dadurch wird sichergestellt, dass mehrere Änderungen
atomar ausgeführt werden.

Zusätzliche Hilfsmethoden wie ClearChangeTracker
oder LogChangeTracker helfen beim Debugging und
bei Integrationstests.


Lernziele
---------

- Verständnis des Unit-of-Work-Patterns
- Trennung zwischen Laden von Aggregaten und Speichern
- Koordination mehrerer Repository-Operationen
- Sicherstellung konsistenter Datenbanktransaktionen
*/