using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Customers._2_Application.Dtos;

public sealed record CustomerDetailsDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   string Email,
   int StatusInt,                 // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   string? ActivatedAt,           // Iso string
   string? RejectedAt,            // Iso string
   int RejectCodeInt,             // see enum none = 0
   Guid? AuditedByEmployeeId,
   string? DeactivatedAt,         // Iso string
   Guid? DeactivatedByEmployeeId,
   AddressDto AddressDto
);
