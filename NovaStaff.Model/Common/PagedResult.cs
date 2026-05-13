public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageIndex,
    int PageSize)
{
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => PageIndex < TotalPages;

    public static PagedResult<T> Empty(int pageIndex, int pageSize)
        => new([], 0, pageIndex, pageSize);
}



