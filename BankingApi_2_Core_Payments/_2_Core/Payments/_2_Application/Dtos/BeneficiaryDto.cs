namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record BeneficiaryDto(
   Guid Id,
   string Name,
   string Iban,
   Guid AccountId
);