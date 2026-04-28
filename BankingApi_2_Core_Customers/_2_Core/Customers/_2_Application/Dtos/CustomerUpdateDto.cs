using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._2_Application.Dtos;

public sealed record CustomerUpdateDto(
   string? Lastname,
   string? CompanyName,
   string? Email,
   AddressDto? AddressDto
);
