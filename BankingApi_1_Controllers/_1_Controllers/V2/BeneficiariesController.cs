using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class BeneficiariesController(
   IAccountReadModel readModel,
   IAccountUseCases useCases,
   ILogger<BeneficiariesController> logger
) : ControllerBase {

   /// <summary>
   /// Returns all beneficiaries of an account.
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of beneficiaries assigned to the account.</returns>
   //[Authorize]
   [HttpGet("accounts/{accountId:guid}/beneficiaries", Name = nameof(GetBeneficiariesByAccountIdAsync))]
   [ProducesResponseType<IEnumerable<BeneficiaryDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ct
   ) {
      const string context = $"{nameof(BeneficiariesController)}.{nameof(GetBeneficiariesByAccountIdAsync)}";

      var result = await readModel.SelectBeneficiariesByAccountIdAsync(accountId, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId });
   }
   
   /// <summary>
   /// Returns a beneficiary of an account by its unique identifier.
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="beneficiaryId">Unique identifier of the beneficiary.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The beneficiary resource if found.</returns>
   //[Authorize]
   [HttpGet("accounts/{accountId:guid}/beneficiaries/{beneficiaryId:guid}", Name = nameof(GetBeneficiaryByIdAsync))]
   [ProducesResponseType<BeneficiaryDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      [FromRoute] Guid accountId,
      [FromRoute] Guid beneficiaryId,
      CancellationToken ct
   ) {
      const string context = $"{nameof(BeneficiariesController)}.{nameof(GetBeneficiaryByIdAsync)}";

      var result = await readModel.FindBeneficiaryByIdAsync(accountId, beneficiaryId, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId, beneficiaryId });
   }
   
   /// <summary>
   /// Returns a beneficiary of an account by its name
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="name">display name of the beneficiary (SQL Like%).</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The beneficiary resource if found.</returns>
   //[Authorize]
   [HttpGet("accounts/{accountId:guid}/beneficiaries/name", Name = nameof(SelectBeneficiariesByNameAsync))]
   [ProducesResponseType<IEnumerable<BeneficiaryDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      [FromRoute] Guid accountId,
      [FromQuery] string name,
      CancellationToken ct
   ) {
      const string context = $"{nameof(BeneficiariesController)}.{nameof(SelectBeneficiariesByNameAsync)}";

      var result = await readModel.SelectBeneficiariesByNameAsync(accountId, name, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId, name });
   }
   
   
   /// <summary>
   /// Adds a beneficiary to an account.
   /// </summary>
   /// <remarks>
   /// A beneficiary belongs to a source account and can later be used as a saved
   /// receiver for transfers.
   /// </remarks>
   /// <param name="accountId">Unique identifier of the source account.</param>
   /// <param name="dto">Beneficiary data used to create the beneficiary.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created beneficiary resource.</returns>
   // [Authorize]
   [HttpPost("accounts/{accountId:guid}/beneficiaries", Name = nameof(CreateBeneficiaryAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<BeneficiaryDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
   public async Task<ActionResult<BeneficiaryDto>> CreateBeneficiaryAsync(
      [FromRoute] Guid accountId,
      [FromBody] BeneficiaryDto dto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(BeneficiariesController)}.{nameof(CreateBeneficiaryAsync)}";

      var result = await useCases.AddBeneficiaryAsync(accountId, dto, ct);

      return this.ToCreatedAtRoute(
         routeName: nameof(GetBeneficiaryByIdAsync),
         routeValues: new {
            accountId,
            beneficiaryId = result.IsSuccess ? result.Value.Id : Guid.Empty
         },
         result,
         logger,
         context
      );
   }
   
   /// <summary>
   /// Removes a beneficiary from an account.
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="beneficiaryId">Unique identifier of the beneficiary.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No content on success.</returns>
   // [Authorize]
   [HttpDelete("accounts/{accountId:guid}/beneficiaries/{beneficiaryId:guid}", Name = nameof(DeleteBeneficiaryAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult> DeleteBeneficiaryAsync(
      [FromRoute] Guid accountId,
      [FromRoute] Guid beneficiaryId,
      CancellationToken ct
   ) {
      const string context = $"{nameof(BeneficiariesController)}.{nameof(DeleteBeneficiaryAsync)}";

      var result = await useCases.RemoveBeneficiaryAsync(accountId, beneficiaryId, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId, beneficiaryId });
   }
}
