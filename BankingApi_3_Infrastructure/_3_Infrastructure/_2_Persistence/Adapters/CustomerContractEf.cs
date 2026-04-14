using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Errors;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal sealed class CustomerContractEf(
   ICustomerRepository repository  
) : ICustomerContract {
   
   public async Task<Result<string>> FindCustomerNameAsync(Guid customerId,
      CancellationToken ct = default) {
      var customer = await repository.FindByIdAsync(customerId, ct);

      return customer is null
         ? Result<string>.Failure(CustomerErrors.NotFound)
         : Result<string>.Success(customer.DisplayName);
   }

   public async Task<Result<bool>> ExistsActiveCustomerAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      var exists = await repository.ExistsActiveAsync(customerId, ct);

      return exists
         ? Result<bool>.Success(exists)
         : Result<bool>.Failure(CustomerErrors.NotFound);
   }
}

