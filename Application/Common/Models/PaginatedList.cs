namespace Application.Common.Models;

public class PaginatedList<T>
{
    public PaginatedList(IReadOnlyCollection<T> Data, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        this.Data = Data;
    }

    public IReadOnlyCollection<T> Data { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize,
        bool SkipPaging = false)
    {
        var count = await source.CountAsync();
        var items = (SkipPaging == false)
                ? await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()
                : await source.ToListAsync()
            ;


        return new PaginatedList<T>(items, count, (SkipPaging == false) ? pageNumber : 1,
            (SkipPaging == false) ? pageSize : count);
    }
}