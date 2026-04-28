using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

// UseCases Facade for Customer aggregate
internal class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcDeactivate deactivateUc,
   CustomerUcUpdate updateUc
): ICustomerUseCases {

   public Task<Result<CustomerDto>> CreateAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      customerCreateDto: customerCreateDto,
      ct: ct
   );
   
   public Task<Result> DeactivateAsync(
      Guid customerId,
      CancellationToken ct
   ) => deactivateUc.ExecuteAsync(customerId, ct);
   
   public Task<Result> UpdateAsync(
      Guid customerId,
      CustomerUpdateDto customerUpdateDto, 
      CancellationToken ct = default
   ) => updateUc.ExecuteAsync(
      customerId: customerId,
      customerUpdateDto: customerUpdateDto,
      ct: ct);
   
}