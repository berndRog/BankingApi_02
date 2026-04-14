namespace BankingApi._2_Core.BuildingBlocks._2_Application.ReadModel;


/// <summary>
/// Paging request for list endpoints.
/// </summary>
public sealed record PageRequest(
   int PageNumber = 1,
   int PageSize = 20
) {
   public int Skip => (PageNumber <= 1 ? 0 : (PageNumber - 1) * PageSize);

   public PageRequest Normalize(int maxPageSize = 200) {
      var page = PageNumber < 1 ? 1 : PageNumber;
      var size = PageSize < 1 ? 20 : PageSize;
      if (size > maxPageSize) size = maxPageSize;
      return this with { PageNumber = page, PageSize = size };
   }
}
