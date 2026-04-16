using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.Query;

public class SearchQuery<TFilter> where TFilter : new()
{
    [FromQuery(Name = "")]
    public TFilter? Filters { get; set; } = new TFilter();

    [FromQuery]
    public Pagination? Pagination { get; set; } = new Pagination();
}