namespace BankingApi._2_Core.BuildingBlocks._2_Application.ReadModel;

public sealed record SortRequest(
   string SortBy = "id",
   SortDirection Direction = SortDirection.Asc
);