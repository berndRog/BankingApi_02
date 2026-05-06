using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._2_Application.Dtos;

public sealed record CustomerCreateDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   string Email,
   string Subject,
   AddressDto AddressDto,
   string? AccountId,
   string? Iban,
   decimal? Balance
);
