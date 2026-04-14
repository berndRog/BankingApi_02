using System.ComponentModel.DataAnnotations.Schema;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._3_Domain.Enums;
namespace BankingApi._2_Core.Payments._3_Domain.ValueObjects;

// Simple Money value object (amount + currency)
[ComplexType]
public sealed record MoneyVo {
   //--- Properties ------------------------------------------------------------
   // Monetary amount (always rounded to 2 decimals)
   public decimal Amount { get; private init; }
   public Currency Currency { get; private init; }
   
   //--- Constructors ----------------------------------------------------------
   // EF Core ctor  
   private MoneyVo() {
      Amount = 0;
      Currency = default!;
   }

   // Domain ctor
   private MoneyVo(
      decimal amount,
      Currency currency
   ) {
      Amount = amount;
      Currency = currency;
   }

   //--- Static Factories ------------------------------------------------------
   public static Result<MoneyVo> Create(
      decimal amount,
      Currency currency
   ) {
      amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
      return Result<MoneyVo>.Success(new MoneyVo(amount, currency));
   }

   public static MoneyVo Zero(Currency currency = Currency.EUR)
      => new(0m, currency);

   // recreate from persisted database value
   internal static MoneyVo FromPersisted(decimal amount, Currency currency)
      => new MoneyVo(amount, currency);

   //--- Operators ----------------------------------------------------------
   public static MoneyVo operator +(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return new MoneyVo(a.Amount + b.Amount, a.Currency);
   }

   public static MoneyVo operator -(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return new MoneyVo(a.Amount - b.Amount, a.Currency);
   }

   public static MoneyVo operator *(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return new MoneyVo(a.Amount * b.Amount, a.Currency);
   }

   public static MoneyVo operator *(MoneyVo a, int quantity) {
      return new MoneyVo(a.Amount * quantity, a.Currency);
   }

   public static bool operator >(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount > b.Amount;
   }

   public static bool operator <(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount < b.Amount;
   }

   public static bool operator >=(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount >= b.Amount;
   }

   public static bool operator <=(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount <= b.Amount;
   }

   //--- Methods -------------------------------------------------------------

   // Ensure both values use the same currency
   private static void EnsureSameCurrency(MoneyVo a, MoneyVo b) {
      if (a.Currency != b.Currency)
         throw new InvalidOperationException("Money currency mismatch.");
   }

   // Human-readable format
   public override string ToString() => $"{Amount:0.00} {Currency}";
}

/*
Didaktik
--------

MoneyVo ist ein Value Object für Geldbeträge.

Es kombiniert zwei Werte:

- Amount (decimal)
- Currency (Enum)

Die Erzeugung erfolgt über zwei Fabrikmethoden:

Create(...)
→ für Benutzereingaben mit Validierung

FromPersisted(...)
→ für Datenbankwerte (sollten bereits gültig sein)

Der Konstruktor ist privat, damit Money nur in einem
gültigen Zustand existieren kann.

Operatoren (+, -, >, <) erlauben eine lesbare
Domain-Logik, z.B.:

balance = balance - amount;


Lernziele
---------

- Verständnis von Value Objects
- Schutz von Domain-Invarianten
- Verwendung von Fabrikmethoden
- Lesbare Domain-Logik durch Operatoren
*/