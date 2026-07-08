using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.Product.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IProductService
    {
        Task<ResultT<PagedResult<ProductListResponse>>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
        Task<ResultT<ProductDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ResultT<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
