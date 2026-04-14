using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.BuildingBlocks;

/// <summary>
/// Centralized logging extensions for Result and Result&lt;T&gt;.
/// Logs failures consistently at the system boundary.
/// </summary>
public static class ResultLoggingExtensions {

   public static Result LogIfFailure(
      this Result result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsFailure && result.Error is not null) {
         logger.LogWarning(
            "{Context} failed. Code={Code}, Title={Title}, Message={Message}, Args={Args}",
            context, result.Error.Code, result.Error.Title, result.Error.Message, args);
      }
      return result;
   }

   public static Result<T> LogIfFailure<T>(
      this Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsFailure && result.Error is not null) {
         logger.LogWarning(
            "{Context} failed. Code={Code}, Title={Title}, Message={Message}, Args={Args}",
            context, result.Error.Code, result.Error.Title, result.Error.Message, args);
      }
      return result;
   }
}


public static class LogRedaction {
   public static string Email(string? email) {
      if (string.IsNullOrWhiteSpace(email)) return "";
      var at = email.IndexOf('@');
      if (at <= 1) return "***";
      return email[..1] + "***" + email[at..];
   }

   public static string Phone(string? phone) {
      if (string.IsNullOrWhiteSpace(phone)) return "";
      return phone.Length <= 3 ? "***" : "***" + phone[^3..];
   }
}

