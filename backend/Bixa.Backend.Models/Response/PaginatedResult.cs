namespace Bixa.Backend.Models.Response;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; }
    public int TotalRecords { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);

    public PaginatedResult(List<T> data, int totalRecords, int currentPage, int pageSize)
    {
        Data = data;
        TotalRecords = totalRecords;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}