using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Category.Requests;
using ECommerce.Application.DTOs.Category.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface ICategoryService
    {
        Task<ResultT<PagedResult<CategoryListResponse>>> GetAllAsync( PaginationRequest request,CancellationToken cancellationToken = default);

        Task<ResultT<CategoryDetailResponse>> GetByIdAsync(int id,CancellationToken cancellationToken = default);

        Task<ResultT<CategoryResponse>> CreateAsync(CreateCategoryRequest request,CancellationToken cancellationToken = default);

        Task<Result> UpdateAsync(int id,UpdateCategoryRequest request,CancellationToken cancellationToken = default);
    
        Task<Result> DeleteAsync( int id,CancellationToken cancellationToken = default);
    }
}
