using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers;

//[ApiVersion("1.0")]
//[ApiVersion("2.0")]
//[Route("banking/v{version:apiVersion}")]
[Route("banking/v2")]
[ApiController]
public sealed class CustomersController(
   ICustomerReadModel readModel,
   ICustomerUseCases useCases,
   ILogger<CustomersController> logger
) : ControllerBase {

   /// <summary>
   /// Creates an activated customer together with a first account.
   /// This endpoint is intended for testing and demo purposes only.
   /// </summary>
   /// <param name="customerCreateDto">Customer data with initial account transferred in the request body.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created customer resource.</returns>
   [HttpPost("customers", Name = nameof(CreateCustomerAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> CreateCustomerAsync(
      [FromBody] CustomerCreateDto customerCreateDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(CreateCustomerAsync)}";
      
      var result = await useCases.CreateAsync(customerCreateDto, ct);

      return this.ToCreatedAtRoute(
         routeName: nameof(GetCustomerByIdAsync),
         routeValues: new { id = customerCreateDto.Id },
         result, logger, context);
   }
   
   /// <summary>
   /// Returns a customer by its unique identifier.
   /// </summary>
   /// <param name="id">Unique identifier of the customer.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The customer resource if found.</returns>
   // [Authorize]
   [HttpGet("customers/{id:guid}", Name = nameof(GetCustomerByIdAsync))]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> GetCustomerByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomerByIdAsync)}";

      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: id);
   }

   /// <summary>
   /// Returns a customer by email address.
   /// </summary>
   /// <param name="email">Email address of the customer.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The customer resource if a matching email address exists.</returns>
   // [Authorize]
   [HttpGet("customers/email", Name = nameof(GetCustomerByEmailAsync))]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> GetCustomerByEmailAsync(
      [FromQuery] string email,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomerByEmailAsync)}";

      var result = await readModel.FindByEmailAsync(email, ct);

      return this.ToActionResult(result, logger, context, args: email);
   }

   /// <summary>
   /// Returns all customers.
   /// </summary>
   /// <remarks>
   /// This endpoint is intended for employees and administrative use cases.
   /// </remarks>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of all customers.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("customers")]
   [ProducesResponseType<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomersAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetAllCustomersAsync)}";

      var result = await readModel.SelectAllAsync(ct);

      return this.ToActionResult(result, logger, context, args: null);
   }
   
   /// <summary>
   /// Returns customers by display name.
   /// </summary>
   /// <remarks>
   /// This endpoint is intended for employees and administrative use cases.
   /// </remarks>
   /// <param name="name">Displayname for SQL %like.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of all customers.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("customers/name")]
   [ProducesResponseType<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersByDisplayNameAsync(
      [FromQuery] string name,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomersByDisplayNameAsync)}";

      var result = await readModel.SelectByDisplayNameAsync(name, ct);

      return this.ToActionResult(result, logger, context, args: name);
   }
}