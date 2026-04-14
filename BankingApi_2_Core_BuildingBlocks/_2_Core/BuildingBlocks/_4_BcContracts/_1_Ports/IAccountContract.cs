using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
namespace BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;

// Contract used by other bounded contexts to interact with the Accounts domain.
// Provides a minimal interface for account-related operations without exposing
// the internal Account aggregate or persistence details.
public interface IAccountContract {

   // Open the initial account for a newly activated customer
   Task<Result<AccountContractDto>> OpenInitialAccountAsync(
      Guid customerId,
      string? accountId,
      string? iban,
      decimal? balance,
      int currency = 1, // EUR
      CancellationToken ct = default
   );

   // Checks whether the customer has no existing accounts
   Task<Result<bool>> HasNoAccountsAsync(
      Guid customerId,
      CancellationToken ct = default
   );
}

/*
Didaktik
--------

Dieses Interface stellt einen Contract zwischen Bounded Contexts dar.

Andere Kontexte (z.B. Customers oder Transfers)
können darüber auf Funktionalitäten des Accounts-Kontexts zugreifen,
ohne direkt auf das Domainmodell zugreifen zu müssen.

Der Contract kapselt dabei zentrale fachliche Operationen:

- Konto eröffnen
- Kontostand oder Snapshot abfragen
- Begünstigte (Beneficiaries) abrufen
- IBAN auf interne Konto-ID auflösen
- Debit- und Credit-Transaktionen durchführen

Wichtig ist, dass nur DTOs zurückgegeben werden und keine
Domain-Objekte. Dadurch bleibt das Domainmodell des
Accounts-Kontexts vollständig gekapselt.

Der Contract bildet eine stabile Schnittstelle
für die Kommunikation zwischen Bounded Contexts.


Lernziele
---------

- Verständnis von Context Contracts in Domain Driven Design
- Reduzierung von Kopplung zwischen Bounded Contexts
- Einsatz von Ports zur Modulkommunikation
- Trennung zwischen Domainmodell und Kontextübergreifenden DTOs
*/