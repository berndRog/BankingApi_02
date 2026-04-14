using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

/// <summary>
/// Employee use case: deactivate a customer relationship
/// </summary>
public sealed class CustomerUcDeactivate(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      // 1) Authorization: must be an employee/admin with the required rights
      if (identityGateway.AdminRights == 0)
         return Result.Failure(CustomerErrors.EmployeeRightsRequired);

      // 2) Validate input
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);

      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      // 4) Domain mutation
      var deactivatedAt = clock.UtcNow;
      var employeeId = ParseEmployeeId(identityGateway.Subject);
      var result = customer.Deactivate(employeeId, deactivatedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcDeactivated", new { customerId, employeeId, deactivatedAt });

      // 5) Persist
      var rows = await unitOfWork.SaveAllChangesAsync("Customer deactivated by employee", ct);
      logger.LogInformation("Account deactivated: {customerId} rows={rows}", customerId, rows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
