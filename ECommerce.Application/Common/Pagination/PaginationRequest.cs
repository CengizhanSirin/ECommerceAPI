namespace ECommerce.Application.Common.Pagination;

public record PaginationRequest( int PageNumber = 1, int PageSize = 10)
{
    public int Skip => (PageNumber - 1) * PageSize;
};
