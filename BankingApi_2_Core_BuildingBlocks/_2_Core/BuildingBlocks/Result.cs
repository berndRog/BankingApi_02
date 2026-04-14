using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks;

// Generic result type for operations that return a value.
// 
// Binary states:
// - Success(value) => IsSuccess == true, Error == DomainErrors.None
// - Failure(error) => IsFailure == true, Value is default
// Design intent:
// - Explicit success/failure without throwing exceptions for domain-level errors.
// - Encourage composing operations via OnSuccess/OnFailure/Fold.
public sealed class Result<T> {
    // Indicates whether the operation failed.
    public bool IsFailure { get; }
    // Convenience flag: true if the operation succeeded.
    public bool IsSuccess => !IsFailure;

    // Error information (DomainErrors.None on success).
    public DomainErrors Error { get; }
    // The successful value (meaningful only when IsSuccess is true).
    public T Value { get; }

    // Private constructor to enforce factory usage.
    private Result(bool isFailure, T value, DomainErrors error) {
        IsFailure = isFailure;
        Value = value;
        Error = error;
    }

    // Creates a successful result with the provided value.
    public static Result<T> Success(T value) =>
        new(isFailure: false, value: value, error: DomainErrors.None);

    // Creates a failed result with the provided domain error.
    // Note: Value is set to default because no valid value exists on failure.
    public static Result<T> Failure(DomainErrors error) =>
        new(isFailure: true, value: default!, error: error);

    // Returns Value if successful and non-null; otherwise returns the provided default value.
    // Useful for optional read scenarios.
    public T GetValueOrDefault(T defaultValue = default!) =>
        IsSuccess && Value is not null ? Value : defaultValue;
    
    // Returns Value if successful; otherwise throws.
    // Use sparingly (typically at boundaries or in tests), not for expected domain failures.
    public T GetValueOrThrow() {
        if (!IsSuccess || Value is null)
            throw new InvalidOperationException($"Result failed: {Error}");
        return Value;
    }

    // Executes an action if the result is successful and Value is non-null.
    // </summary>
    public Result<T> OnSuccess(Action<T> action) {
        if (IsSuccess && Value is not null)
            action(Value);

        return this;
    }

    // Executes an action if the result is a failure.
    // Returns this result for fluent chaining.
    public Result<T> OnFailure(Action<DomainErrors> action) {
        if (IsFailure)
            action(Error);

        return this;
    }
    
    // Maps a result into a single value (expression form).
    // - onSuccess is called when successful
    // - onFailure is called when failed
    public TResult Fold<TResult>(
        Func<T, TResult> onSuccess,
        Func<DomainErrors, TResult> onFailure) {
        return IsSuccess && Value is not null
            ? onSuccess(Value)
            : onFailure(Error);
    }

    // Transforms the successful value using the provided mapper function.
    //
    // Behavior:
    // - If Success → applies mapper to Value and wraps result in Result<TResult>.Success.
    // - If Failure → propagates the existing error without invoking mapper.
    // Purpose:
    // - Enables functional-style value transformation.
    // - Does NOT allow the mapper to return another Result.
    // - Also known as "Select" in functional terminology.
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) {
        return IsSuccess && Value is not null
            ? Result<TResult>.Success(mapper(Value))
            : Result<TResult>.Failure(Error);
    }


    // Chains another operation that itself returns a Result.
    //
    // Behavior:
    // - If Success → invokes binder(Value) and returns its Result directly.
    // - If Failure → propagates the existing error without invoking binder.
    // Purpose:
    // - Enables composition of operations that may fail.
    // - Prevents nested Result<Result<T>> structures.
    // - Also known as "FlatMap" or "SelectMany" in functional terminology.
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) {
        return IsSuccess && Value is not null
            ? binder(Value)
            : Result<TResult>.Failure(Error);
    }
}

// Non-generic result type for operations that return no value.
// 
// Semantics:
// - Success  => no error (DomainErrors.None)
// - Failure  => an error is present
// 
// Design intent:
// - Avoid exceptions for expected business/validation failures.
// - Make control flow explicit and testable.
public sealed class Result {
    // Indicates whether the operation succeeded.
    public bool IsSuccess { get; }
    // Convenience flag: true if the operation failed.
    public bool IsFailure => !IsSuccess;

    // Error information (DomainErrors.None on success).
    public DomainErrors Error { get; }
    
    // Private constructor to enforce factory usage.
    private Result(bool isSuccess, DomainErrors error) {
        IsSuccess = isSuccess;
        Error = error;
    }

    // Creates a successful result with DomainErrors.None.
    public static Result Success() =>
        new(isSuccess: true, error: DomainErrors.None);

    // Creates a failed result with the provided domain error.
    public static Result Failure(DomainErrors error) =>
        new(isSuccess: false, error: error);
}


/*
   ================================================================================
   DIDAKTIK UND LERNZIELE ( Result<T> / Map / Bind und Result)
   ================================================================================
   1) Exceptions sind kein Kontrollfluss für erwartete Fehler
      - Validierungsfehler, "NotFound", "NotAuthorized", "InvalidState"
        sind normaler Fach- und Anwendungsfluss.
      - Result modelliert diese Zustände explizit statt implizit über Exceptions.
      - Fehler werden zu einem bewussten, typisierten Rückgabewert.
   
   2) Binäre Zustände sauber modellieren
      - Erfolg/Failure als klarer, endlicher Zustandsraum.
      - Kein "halb erfolgreich" ohne definierte Regel.
      - DomainErrors.None signalisiert explizit: kein Fehler.
   
   3) Trennung von Fachlichkeit und Technik stärken
      - Domänen- und Anwendungslogik liefert Result zurück.
      - Infrastruktur (Controller/API) übersetzt Result in HTTP
        (z.B. ProblemDetails).
      - Die Domäne bleibt framework-unabhängig und testbar.
   
   4) Funktionale Komposition verstehen (Map / Bind)
      - Map:   transformiert einen erfolgreichen Wert (T → TResult).
      - Bind:  verkettet Operationen, die selbst Result zurückgeben
               (T → Result<TResult>).
      - Bind verhindert verschachtelte Result<Result<T>>-Strukturen.
      - Fehler werden automatisch propagiert ("Railway-Oriented Programming").
   
   5) Pipeline-Denken statt Try/Catch-Spaghetti
      - OnSuccess / OnFailure / Fold / Map / Bind ermöglichen
        lesbare Verarbeitungsketten.
      - Erfolgs- und Fehlerpfad sind klar und deterministisch modelliert.
      - Reduziert Boilerplate-Code in UseCases erheblich.
   
   6) Testbarkeit erhöhen
      - Unit Tests prüfen IsSuccess/IsFailure und Error-Codes direkt.
      - Kein Exception-Handling im Test erforderlich.
      - Fehlerfälle werden genauso selbstverständlich getestet wie Erfolgsfälle.
   
   7) Typische Einsatzorte im Projekt (z.B. BankingApi)
      - ValueObject.Create(...)        → Result<T>
      - UseCase.ExecuteAsync(...)      → Result<T> oder Result
      - ReadModel.Find...              → Result<T> (NotFound, Validation)
      - ID-Parsing / Validierung       → Result<T>
   
   Zentrale Lernziele:
   Studierende sollen verstehen, dass Fehlerbehandlung kein Nebeneffekt
   (über Exceptions) sein muss, sondern explizit, typisiert und
   komponierbar modelliert werden kann.
   
   Result ist ein vertraglicher Rückgabewert:
   Er sagt klar, ob etwas funktioniert hat – und wenn nicht, warum.
   
   ================================================================================
 */