using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.Extensions;

// Translates domain/application Result objects into HTTP responses.
// Controllers stay thin and delegate protocol mapping to this adapter.
public static class ControllerExtensions {

   // Query / command with response body
   // Success  -> 200 OK + body
   // Failure  -> mapped ProblemDetails response
   public static ActionResult<T> ToActionResult<T>(
      this ControllerBase controller,
      Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      ArgumentNullException.ThrowIfNull(controller);
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(logger);
      ArgumentException.ThrowIfNullOrWhiteSpace(context);

      if (result.IsSuccess)
         return controller.Ok(result.Value);

      return controller.ToProblemActionResult(result.Error, logger, context, args);
   }

   // Command without response body
   // Success  -> 204 NoContent
   // Failure  -> mapped ProblemDetails response
   public static ActionResult ToActionResult(
      this ControllerBase controller,
      Result result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      ArgumentNullException.ThrowIfNull(controller);
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(logger);
      ArgumentException.ThrowIfNullOrWhiteSpace(context);

      if (result.IsSuccess)
         return controller.NoContent();

      return controller.ToProblemActionResult(result.Error, logger, context, args);
   }

   // Create resource
   // Success  -> 201 Created + Location header + body
   // Failure  -> mapped ProblemDetails response
   public static ActionResult<T> ToCreatedAtRoute<T>(
      this ControllerBase controller,
      string routeName,
      object routeValues,
      Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      ArgumentNullException.ThrowIfNull(controller);
      ArgumentException.ThrowIfNullOrWhiteSpace(routeName);
      ArgumentNullException.ThrowIfNull(routeValues);
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(logger);
      ArgumentException.ThrowIfNullOrWhiteSpace(context);

      if (result.IsFailure)
         return controller.ToProblemActionResult(result.Error, logger, context, args);

      var routeValueDictionary = new RouteValueDictionary(routeValues);
      AddCurrentApiVersion(controller, routeValueDictionary);

      try {
         return controller.CreatedAtRoute(
            routeName: routeName,
            routeValues: routeValueDictionary,
            value: result.Value
         );
      }
      catch (Exception ex) {
         logger.LogError(
            ex,
            "CreatedAtRoute failed in {Context}. RouteName: {RouteName}, RouteValues: {@RouteValues}",
            context,
            routeName,
            routeValueDictionary
         );

         // Fallback: resource was created, but route generation failed
         return controller.StatusCode(StatusCodes.Status201Created, result.Value);
      }
   }

   private static void AddCurrentApiVersion(
      ControllerBase controller,
      RouteValueDictionary routeValues
   ) {
      if (routeValues.ContainsKey("version"))
         return;

      var routeVersion = controller.RouteData.Values["version"]?.ToString();
      if (!string.IsNullOrWhiteSpace(routeVersion)) {
         routeValues["version"] = routeVersion;
         return;
      }

      var requestedVersion = controller.HttpContext.GetRequestedApiVersion();
      if (requestedVersion is not null)
         routeValues["version"] = requestedVersion.ToString();
   }

   // Centralized mapping of DomainError -> HTTP ProblemDetails response
   private static ActionResult ToProblemActionResult(
      this ControllerBase controller,
      DomainErrors error,
      ILogger logger,
      string context,
      object? args = null
   ) {
      ArgumentNullException.ThrowIfNull(controller);
      ArgumentNullException.ThrowIfNull(error);
      ArgumentNullException.ThrowIfNull(logger);
      ArgumentException.ThrowIfNullOrWhiteSpace(context);

      var statusCode = ToHttpStatusCode(error.Code);

      logger.Log(
         ToLogLevel(statusCode),
         "Request failed in {Context}. ErrorCode: {ErrorCode}, Title: {Title}, Message: {Message}, Args: {@Args}",
         context,
         error.Code,
         error.Title,
         error.Message,
         args
      );

      var problemDetails = new ProblemDetails {
         Title = error.Title,
         Detail = error.Message,
         Status = statusCode,
         Type = $"https://httpstatuses.com/{statusCode}"
      };

      problemDetails.Extensions["code"] = error.Code.ToString();
      problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

      return statusCode switch {
         StatusCodes.Status400BadRequest =>
            controller.BadRequest(problemDetails),

         StatusCodes.Status401Unauthorized =>
            controller.Unauthorized(problemDetails),

         StatusCodes.Status403Forbidden =>
            controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),

         StatusCodes.Status404NotFound =>
            controller.NotFound(problemDetails),

         StatusCodes.Status409Conflict =>
            controller.Conflict(problemDetails),

         StatusCodes.Status415UnsupportedMediaType =>
            controller.StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails),

         StatusCodes.Status422UnprocessableEntity =>
            controller.UnprocessableEntity(problemDetails),

         _ =>
            controller.StatusCode(statusCode, problemDetails)
      };
   }
   // Domain error code -> HTTP status code
   public static int ToHttpStatusCode(ErrorCode errorCode) =>
      errorCode switch {
         ErrorCode.BadRequest           => StatusCodes.Status400BadRequest,
         ErrorCode.Unauthorized         => StatusCodes.Status401Unauthorized,
         ErrorCode.Forbidden            => StatusCodes.Status403Forbidden,
         ErrorCode.NotFound             => StatusCodes.Status404NotFound,
         ErrorCode.Conflict             => StatusCodes.Status409Conflict,
         ErrorCode.UnsupportedMediaType => StatusCodes.Status415UnsupportedMediaType,
         ErrorCode.UnprocessableEntity  => StatusCodes.Status422UnprocessableEntity,
         _                              => StatusCodes.Status400BadRequest
      };

   private static LogLevel ToLogLevel(int statusCode) =>
      statusCode switch {
         StatusCodes.Status400BadRequest          => LogLevel.Information,
         StatusCodes.Status401Unauthorized        => LogLevel.Information,
         StatusCodes.Status403Forbidden           => LogLevel.Information,
         StatusCodes.Status404NotFound            => LogLevel.Information,
         StatusCodes.Status409Conflict            => LogLevel.Information,
         StatusCodes.Status415UnsupportedMediaType => LogLevel.Information,
         StatusCodes.Status422UnprocessableEntity => LogLevel.Information,
         _ when statusCode >= 500                 => LogLevel.Error,
         _                                        => LogLevel.Warning
      };
}
