using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks;

public static class SubjectCheck {

   public static Result<string> Run(string input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidIdentitySubject);
      if (input.Length > 200)
         return Result<string>.Failure(CommonErrors.InvalidIdentitySubject);

      // Identity subject as issued by IAM (opaque, not interpreted).
      return Result<string>.Success(input);
   }
}