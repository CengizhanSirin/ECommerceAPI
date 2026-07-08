using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Brand.Requests;
using ECommerce.Application.DTOs.Brand.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IBrandService
    {
        Task<ResultT<PagedResult<BrandListResponse>>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);

        Task<ResultT<BrandDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ResultT<BrandResponse>> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);

        Task<Result> UpdateAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken = default);

        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    }
}
