namespace BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;

// Contract used by other bounded contexts to interact with the Customer domain.
// Provides a minimal interface for customer-related operations without exposing
// the internal Customer aggregate or persistence details.
public interface ICustomerContract {
   
   // Find the customer displayname by customerId
   Task<Result<string>> FindCustomerNameAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // Exits customer and is active?
   Task<Result<bool>> ExistsActiveCustomerAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
   
}
