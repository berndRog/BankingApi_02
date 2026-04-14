namespace BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;

[Flags]
public enum AdminRights {

   None = 0,

   // Reports / Audit
   ViewReports = 1 << 0,    // 0000001

   // Customers / Customers (KYC approval etc.)
   ViewCustomers   = 1 << 1,
   // activate/reject/deactivate employees
   ManageCustomers = 1 << 2, 

   // Accounts & Beneficiaries
   ViewAccounts   =  1 << 3,     //    8
   // activate/deactivate accounts
   ManageAccounts = 1 << 4,     //   16

   // Transfers & Transactions
   ViewTransfers   = 1 << 5,    //   32
   // manual review / cancel (if applicable)
   ManageTransfers = 1 << 6,    //   64
   
   // Employees
   ViewEmployees   = 1 << 7,    //  128 
   ManageEmployees = 1 << 8     //  256  all 511
}

/*
 * Beispiel: Rechte kombinieren
 * ----------------------------
 * var rights =
 *    AdminRights.ViewEmployees |
 *    AdminRights.ManageEmployees |
 *    AdminRights.ViewAccounts;
 * AminRights = rights; 
 *    
 * Einzelnes Recht prüfen
 * ----------------------
 * bool hasRight =
 *   (rights & AdminRights.ManageEmployees) == AdminRights.ManageEmployees;
 *  
 * Mehrere Rechte prüfen
 * ----------------------  
 * var required = AdminRights.ViewEmployees | AdminRights.ManageEmployees;
 * bool hasRights = (rights & required) == required;
 */


/* =====================================================================
 * AdminRights – Verarbeitung & Architekturhinweise
 * =====================================================================
 *
 * Grundidee:
 * ----------
 * AdminRights werden als Bitmaske in einem int gespeichert.
 * Jedes einzelne Recht belegt genau ein Bit (1 << n).
 *
 * Dadurch können mehrere Rechte effizient in einer einzigen
 * Ganzzahl kombiniert und gespeichert werden.
 *
 *
 * Warum 1 << n?
 * -------------
 * - 1 << n setzt genau das n-te Bit im Integer
 * - Jedes Recht ist eindeutig (keine Überlappung)
 * - Die Definition ist selbsterklärend („Bitposition“)
 * - Neue Rechte können jederzeit ergänzt werden
 *
 * Beispiel:
 * ----------
 * 1 << 0 = 0000 0001  (Bit 0)
 * 1 << 1 = 0000 0010  (Bit 1)
 * 1 << 2 = 0000 0100  (Bit 2)
 *
 *
 * Rechte kombinieren:
 * -------------------
 * Mehrere Rechte werden mit bitweisem ODER (|) kombiniert:
 *
 *   int rights =
 *      ViewCars |
 *      ManageCars |
 *      ViewBookings;
 *
 * Ergebnis:
 * - Alle zugehörigen Bits sind gesetzt
 *
 *
 * Rechte prüfen:
 * --------------
 * Ein einzelnes Recht wird mit bitweisem UND (&) geprüft:
 *
 *   bool hasManageCars =
 *      (rights & ManageCars) == ManageCars;
 *
 * Bedeutung:
 * - Ist das entsprechende Bit gesetzt, besitzt der Benutzer das Recht
 *
 *
 * Einzelne Rechte ermitteln:
 * --------------------------
 * Um alle gesetzten Rechte zu bestimmen, wird über alle bekannten
 * Flags iteriert und jeweils geprüft, ob das Bit gesetzt ist.
 *
 * Dies wird z.B. benötigt für:
 * - UI-Menüfreischaltung
 * - Anzeige von Benutzerrechten
 * - Ableitung von Claims / Policies
 *
 *
 * Abgrenzung:
 * -----------
 * - AdminRights modellieren fachliche Berechtigungen
 * - Sie sind KEINE technischen Rollen (z.B. ASP.NET Roles)
 * - Sie sind KEINE API-Endpunkt-Berechtigungen
 *
 *
 * Einsatz im System:
 * ------------------
 * - Domain / Datenbank:
 *   - Speicherung als int (AdminRights)
 *
 * - Application Layer:
 *   - Bitweise Prüfung der Rechte
 *
 * - Authentifizierung / Token:
 *   - Ableitung von Claims aus den gesetzten Bits
 *
 * - Autorisierung:
 *   - Policies prüfen auf erforderliche Claims
 *
 *
 * Wichtiger Merksatz:
 * ------------------
 * AdminRights sind eine Menge von Fähigkeiten,
 * modelliert als Bitmaske – nicht als einzelne Rollen.
 *
 * =====================================================================
 */

