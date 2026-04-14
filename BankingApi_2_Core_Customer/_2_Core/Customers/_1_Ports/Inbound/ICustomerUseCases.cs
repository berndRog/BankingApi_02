using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Customers._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

// Application port defining all command use cases for the Customers bounded context.
// Represents the write side of the application (CQRS command side).
// Used by API controllers to trigger state changes in the Customer domain.
public interface ICustomerUseCases {

   // Create a fully initialized customer
   // And also create the first account
   Task<Result<CustomerDto>> CreateAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct = default
   );
   
   // Employee action: deactivate an existing customer
   Task<Result> DeactivateAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   
   // Change the customer's profile data
   Task<Result> UpdateAsync(
      CustomerDto customerDto,
      CancellationToken ct = default
   );
}
