namespace BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;

public record AccountContractDto(
   Guid Id,
   string Iban,
   Guid CustomerId
); 
