namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record AccountDto(
   Guid Id,
   string Iban,
   decimal Balance,
   int Currency,
   Guid CustomerId
); 
