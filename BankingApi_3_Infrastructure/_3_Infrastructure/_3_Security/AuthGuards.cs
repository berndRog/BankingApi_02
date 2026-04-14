using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._3_Infrastructure._3_Security;

public static class AuthGuards {
   // Ensures the current identity represents an employee/admin.
   // Used for directory-style read access (e.g. FindById, FindByEmail).
   public static Result EnsureEmployee(IIdentityGateway identity) {
      return identity.AdminRights != 0
         ? Result.Success()
         : Result.Failure(DomainErrors.Forbidden);
   }
   
   // Ensures the current identity represents a customer (self-service).
   // Used for profile access and self-owned operations.
   public static Result EnsureCustomer(IIdentityGateway identity) {
      return identity.AdminRights == 0
         ? Result.Success()
         : Result.Failure(DomainErrors.Forbidden);
   }
}