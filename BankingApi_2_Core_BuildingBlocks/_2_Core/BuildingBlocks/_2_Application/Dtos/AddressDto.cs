namespace BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;

public sealed record AddressDto(
   string  Street,
   string  PostalCode,
   string  City,
   string? Country
);
