// using BankingApi._2_Core.BuildingBlocks;
// using BankingApi._2_Core.BuildingBlocks._3_Domain;
// using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
// using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
// using Microsoft.AspNetCore.Mvc;
// namespace BankingApi._1_Controllers.Extensions;
//
// public static class ResultApiExtensions {
//
//    /// <summary>
//    /// Converts a domain <see cref="Result{T}"/> into an ASP.NET Core <see cref="ActionResult"/>.
//    ///
//    /// PURPOSE
//    /// -------
//    /// This overload is used for queries or commands that return a value.
//    /// The returned value becomes the HTTP response body.
//    ///
//    /// BEHAVIOR
//    /// --------
//    /// Success
//    ///     → HTTP 200 OK
//    ///     → Response body contains the value (T)
//    ///
//    /// Failure
//    ///     → DomainError is translated into an HTTP status code
//    ///     → Returned as RFC7807 ProblemDetails
//    ///     → Failure is logged
//    ///
//    /// COMMAND vs QUERY SEMANTICS
//    /// --------------------------
//    /// Result        -> command without response body -> 204 NoContent
//    /// Result<T>     -> query or read model           -> 200 OK + body
//    ///
//    /// This keeps controller actions trivial:
//    ///     return this.ToActionResult(result, logger, "GetOwner");
//    ///
//    /// IMPORTANT
//    /// ---------
//    /// The controller does not interpret the domain state.
//    /// The UseCase already decided success or failure.
//    /// This method only translates the outcome into HTTP.
//    ///
//    /// EXAMPLES
//    /// --------
//    /// GetCustomerById        -> 200 OK + CustomerDto
//    /// AccountNotFound     -> 404 NotFound
//    /// InvalidFilter       -> 400 BadRequest
//    /// </summary>
//    /// <typeparam name="T">Type of the response body</typeparam>
//    /// <param name="controller">Calling controller (extension target)</param>
//    /// <param name="result">Domain/application result</param>
//    /// <param name="logger">Logger for structured error logging</param>
//    /// <param name="context">Logical operation name (use case)</param>
//    /// <param name="args">Optional structured log arguments</param>
//    /// <returns>HTTP response representing the domain outcome</returns>
//    public static ActionResult ToActionResult<T>(
//       this ControllerBase controller,
//       Result<T> result,
//       ILogger logger,
//       string context,
//       object? args = null
//    ) {
//       // Success -> HTTP 200 OK with response body
//       if (result.IsSuccess)
//          return controller.Ok(result.Value);
//
//       // Failure -> log and translate DomainErrors into HTTP status codes
//       result.LogIfFailure(logger, context, args);
//
//       var error = result.Error;
//
//       // RFC 7807 ProblemDetails response
//       var problemDetails = new ProblemDetails {
//          Title = error.Title,
//          Detail = error.Message,
//          Status = error.Code.ToHttpStatusCode()
//       };
//
//       return error.Code switch {
//          ErrorCode.BadRequest =>
//             controller.BadRequest(problemDetails),
//
//          ErrorCode.Unauthorized =>
//             controller.Unauthorized(problemDetails),
//
//          ErrorCode.Forbidden =>
//             controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),
//
//          ErrorCode.NotFound =>
//             controller.NotFound(problemDetails),
//
//          ErrorCode.Conflict =>
//             controller.Conflict(problemDetails),
//
//          ErrorCode.UnsupportedMediaType =>
//             controller.StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails),
//
//          ErrorCode.UnprocessableEntity =>
//             controller.UnprocessableEntity(problemDetails),
//
//          _ =>
//             controller.BadRequest(problemDetails)
//       };
//    }
//
//    /// <summary>
//    /// Converts a domain <see cref="Result"/> into an ASP.NET Core <see cref="ActionResult"/>.
//    ///
//    /// PURPOSE
//    /// -------
//    /// This method forms the boundary between the application/domain layer and HTTP.
//    /// The domain returns semantic results (success/failure + DomainError),
//    /// while the controller must return protocol-specific responses (status codes + body).
//    ///
//    /// BEHAVIOR
//    /// --------
//    /// Success (Result without value)
//    ///     → HTTP 204 NoContent
//    ///     Typical for commands:
//    ///         CreateOwner
//    ///         ActivateAccount
//    ///         SendMoney
//    ///
//    /// Failure
//    ///     → DomainError is translated into an HTTP status code
//    ///     → Returned as RFC7807 ProblemDetails
//    ///     → Failure is logged (important for diagnostics & observability)
//    ///
//    /// DESIGN RATIONALE
//    /// ----------------
//    /// - The domain must not know HTTP
//    /// - Controllers must not implement business rules
//    /// - All error handling must be consistent across the entire API
//    ///
//    /// This method therefore acts as a protocol adapter.
//    ///
//    /// IMPORTANT:
//    ///     Domain errors are NOT exceptions.
//    ///     They represent expected business outcomes.
//    ///
//    /// EXAMPLES
//    /// --------
//    /// EmailAlreadyExists      -> 409 Conflict
//    /// OwnerNotFound           -> 404 NotFound
//    /// InvalidStateTransition  -> 422 UnprocessableEntity
//    /// ValidationError         -> 400 BadRequest
//    ///
//    /// </summary>
//    /// <param name="controller">The calling ASP.NET controller (extension target)</param>
//    /// <param name="result">Domain/application result object</param>
//    /// <param name="logger">Logger used to record failures</param>
//    /// <param name="context">Logical operation name (use case) for structured logging</param>
//    /// <param name="args">Optional structured log arguments</param>
//    /// <returns>HTTP response representing the domain outcome</returns>
//    public static ActionResult ToActionResult(
//       this ControllerBase controller,
//       Result result,
//       ILogger logger,
//       string context,
//       object? args = null
//    ) {
//       // Success -> HTTP 204 NoContent (typical for commands without response body)
//       if (result.IsSuccess)
//          return controller.NoContent();
//
//       // Failure -> log and translate DomainErrors into HTTP status codes
//       result.LogIfFailure(logger, context, args);
//
//       var error = result.Error;
//
//       // RFC 7807 ProblemDetails response
//       var problemDetails = new ProblemDetails {
//          Title = error.Title,
//          Detail = error.Message,
//          Status = error.Code.ToHttpStatusCode()
//       };
//
//       return error.Code switch {
//          ErrorCode.BadRequest =>
//             controller.BadRequest(problemDetails),
//
//          ErrorCode.Unauthorized =>
//             controller.Unauthorized(problemDetails),
//
//          ErrorCode.Forbidden =>
//             controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),
//
//          ErrorCode.NotFound =>
//             controller.NotFound(problemDetails),
//
//          ErrorCode.Conflict =>
//             controller.Conflict(problemDetails),
//
//          ErrorCode.UnsupportedMediaType =>
//             controller.StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails),
//
//          ErrorCode.UnprocessableEntity =>
//             controller.UnprocessableEntity(problemDetails),
//
//          _ =>
//             controller.BadRequest(problemDetails)
//       };
//    }
//
//    
//    /// Converts a successful creation Result into HTTP 201 Created.
//    ///
//    /// This method represents the REST "Create Resource" response semantics.
//    ///
//    /// DOMAIN VIEW
//    /// -----------
//    /// The UseCase returns:
//    ///     Result<T>
//    ///
//    /// Success  -> A new domain object was created
//    /// Failure  -> A business rule prevented creation (NOT an exception)
//    ///
//    /// HTTP VIEW
//    /// ---------
//    /// On success the API MUST return:
//    ///     201 Created
//    ///     Location header pointing to the new resource
//    ///     Response body containing the created DTO
//    ///
//    
//     
//    public static ActionResult ToCreatedAtRoute<T>(
//       this ControllerBase controller,
//       string routeName,
//       object? routeValues,
//       Result<T> result,
//       ILogger logger,
//       string context,
//       object? args = null
//    ) {
//       // Domain failure -> map to HTTP error response (400–422)
//       if (result.IsFailure)
//          return controller.ToActionResult(result, logger, context, args);
//
//       // NEVER pass null to CreatedAtRoute
//       routeValues ??= new { };
//       
//       // Domain success -> HTTP 201 + Location header
//       try {
//          logger.LogInformation(
//             "Resource created successfully. Returning 201 Created. " +
//             "Route: {RouteName}, RouteValues: {@RouteValues}, Result: {@Result}",
//             routeName, routeValues, result.Value);
//          return controller.CreatedAtRoute(
//             routeName: routeName, 
//             routeValues: routeValues, 
//             value: result.Value
//          );
//       }
//       catch (Exception ex) {
//          logger.LogError(ex, 
//             "Failed to log creation of resource. " +
//             "Route: {RouteName}, RouteValues: {@RouteValues}",
//             routeName, routeValues); 
//          // Fallback: 201 without Location (better than crashing)
//          return controller.StatusCode(StatusCodes.Status201Created, result.Value);
//       }
//    }
// }
//
// /*
// ================================================================================
// DIDAKTIK & LERNZIELE
// ================================================================================
//
// Dieses File implementiert die Übersetzungsschicht zwischen:
//
//     DOMAIN (UseCases liefern Result<T>)
//                 ↓
//     APPLICATION / API BOUNDARY
//                 ↓
//     HTTP / REST Welt
//
// Ziel ist die strikte Trennung:
//
//     Domain kennt KEIN HTTP
//     Controller kennt KEINE Businesslogik
//
// Der Controller wird dadurch zu einem reinen Adapter.
//
// ----------------------------------------------------------------------
// WAS STUDIERENDE HIER LERNEN SOLLEN
// ----------------------------------------------------------------------
//
// 1) Fehler gehören zur Fachlogik – nicht zur Technik
// ---------------------------------------------------
// In klassischen CRUD-Anwendungen werden Fehler oft als Exceptions behandelt.
// In einem DDD-System sind viele Fehler aber erwartbares Verhalten:
//
//     Konto nicht gefunden
//     Email bereits vergeben
//     Überweisung nicht erlaubt
//     Statusübergang ungültig
//
// Diese sind KEINE technischen Fehler → sondern Domänenentscheidungen.
//
// Darum:
//     Domain -> Result / DomainError
//     Nicht -> Exception
//
// ----------------------------------------------------------------------
// 2) Entkopplung von Domain und Transportprotokoll
// ------------------------------------------------
// Die Domain darf NICHT wissen:
//
//     HTTP
//     REST
//     Statuscodes
//     Controller
//     JSON
//     ASP.NET
//
// Die API darf NICHT wissen:
//
//     Validierungsregeln im Detail
//     Aggregatzustände
//     Geschäftslogik
//
// Dieses File ist der "Anti-Corruption Layer" zwischen beiden Welten.
//
// ----------------------------------------------------------------------
// 3) Konsistentes API-Verhalten (Uniform Error Handling)
// ------------------------------------------------------
// Alle Controller bekommen automatisch:
//
//     gleiche Fehlerstruktur
//     gleiche Statuscodes
//     gleiche Loggingstrategie
//     gleiche ProblemDetails (RFC7807)
//
// Dadurch entstehen:
//
//     wartbare APIs
//     testbare APIs
//     dokumentierbare APIs
//     vorhersehbare Clients
//
// ----------------------------------------------------------------------
// 4) Architekturprinzipien
// ------------------------------------------------------
// Hier werden mehrere zentrale Architekturideen praktisch umgesetzt:
//
// - Clean Architecture → Interface Adapter Layer
// - Hexagonale Architektur → Primary Adapter (HTTP)
// - DDD → Application Service liefert Result
// - SOLID → SRP (Controller nur Mapping)
// - DRY → zentrale Fehlerübersetzung
// - Tell, don't ask → UseCase entscheidet Erfolg/Misserfolg
//
// ----------------------------------------------------------------------
// KERNGEDANKE FÜR STUDIERENDE
// ------------------------------------------------------
// Controller sind KEINE Businesslogik.
// Controller sind Protokolladapter.
//
// Die Fachlichkeit endet im UseCase.
// HTTP beginnt erst danach.
//
// Dieses File ist genau die Grenze zwischen beiden Welten.
// */