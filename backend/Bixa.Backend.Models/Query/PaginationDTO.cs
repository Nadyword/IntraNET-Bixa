using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.Query;

public class Pagination
{
    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public int PageSize { get; set; } = 100;
}
