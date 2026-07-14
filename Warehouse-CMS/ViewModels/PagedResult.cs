namespace Warehouse_CMS.ViewModels
{
    /// <summary>
    /// A single page of results plus the paging metadata a view needs to render controls.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
