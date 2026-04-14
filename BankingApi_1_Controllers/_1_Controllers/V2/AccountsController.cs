using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class AccountsController(
   IAccountReadModel readModel,
   IAccountUseCases useCases,
   ILogger<AccountsController> logger
) : ControllerBase {

   /// <summary>
   /// Returns an account by its unique identifier.
   /// </summary>
   /// <param name="id">Unique identifier of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The account resource if found.</returns>
   // [Authorize]
   [HttpGet("accounts/{id:guid}", Name = nameof(GetAccountByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto>> GetAccountByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountByIdAsync)}";

      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }

   /// <summary>
   /// Returns an account by IBAN.
   /// </summary>
   /// <param name="iban">IBAN of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The account resource if found.</returns>
   // [Authorize]
   [HttpGet("accounts/iban", Name = nameof(GetAccountByIbanAsync))]
   [Produces("application/json")]
   [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto>> GetAccountByIbanAsync(
      [FromQuery] string iban,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountByIbanAsync)}";

      var result = await readModel.FindByIbanAsync(iban, ct);

      return this.ToActionResult(result, logger, context, args: new { iban });
   }

   /// <summary>
   /// Returns all accounts of a specific customer.
   /// </summary>
   /// <param name="customerId">Unique identifier of the customer.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of accounts belonging to the given customer.</returns>
   // [Authorize]
   [HttpGet("customers/{customerId:guid}/accounts", Name = nameof(GetAccountsByCustomerIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IEnumerable<AccountDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByCustomerIdAsync(
      [FromRoute] Guid customerId,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountsByCustomerIdAsync)}";

      var result = await readModel.SelectByCustomerIdAsync(customerId, ct);

      return this.ToActionResult(result, logger, context, args: new { customerId });
   }
   
   /// <summary>
   /// Creates a new account for a customer.
   /// </summary>
   /// <remarks>
   /// This endpoint creates a new account resource and returns the created account
   /// together with a Location header pointing to the account resource.
   /// </remarks>
   /// <param name="customerId">Id of the Customer who ownes the account.</param>
   /// <param name="accounDto">Account data used to create the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created account resource.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpPost("customers/{customerId}/accounts", Name = nameof(CreateAccountAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
   public async Task<ActionResult<AccountDto>> CreateAccountAsync(
      [FromRoute] Guid customerId,
      [FromBody] AccountDto accounDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(CreateAccountAsync)}";

      var result = await useCases.CreateAsync(customerId, accounDto, ct);

      return this.ToCreatedAtRoute(
         routeName: nameof(GetAccountByIdAsync),
         routeValues: new { id = result.IsSuccess ? result.Value.Id : Guid.Empty },
         result,
         logger,
         context
      );
   }
   
   /// <summary>
   /// Deactivates an account.
   /// </summary>
   /// <remarks>
   /// This endpoint changes the lifecycle state of the account and does not return
   /// a response body on success.
   /// </remarks>
   /// <param name="id">Unique identifier of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No content on success.</returns>
   //[Authorize(Policy = "EmployeesOnly")]
   [HttpPut("accounts/{id:guid}", Name = nameof(DeactivateAccountAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
   public async Task<ActionResult> DeactivateAccountAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(DeactivateAccountAsync)}";

      var result = await useCases.DeactivateAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }

}