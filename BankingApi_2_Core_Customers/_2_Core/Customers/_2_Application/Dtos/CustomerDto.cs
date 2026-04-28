using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._2_Application.Dtos;

public sealed record CustomerDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   int StatusInt,            // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   string Email,
   AddressDto AddressDto
);
