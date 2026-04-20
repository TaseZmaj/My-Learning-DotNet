namespace EventsManagement.Web.Request;

public class PaginateRequest
{
    public const int MaxPageSize = 50;
    public int _pageSize = 10;

    public int PageNumber { get; set; } = 0;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize || value < 0) ? MaxPageSize : value;
    }
}