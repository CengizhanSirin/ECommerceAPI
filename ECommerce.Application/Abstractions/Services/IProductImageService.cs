using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.ProductImage.Requests;
using ECommerce.Application.DTOs.ProductImage.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IProductImageService
    {
        Task<ResultT<IReadOnlyList<ProductImageResponse>>> GetByProductIdAsync(int productId,  CancellationToken cancellationToken = default);
        Task<ResultT<ProductImageResponse>> CreateAsync(int productId,CreateProductImageRequest request,CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync( int productId,int imageId, UpdateProductImageRequest request,CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int productId,int imageId, CancellationToken cancellationToken = default);
        Task<Result> SetMainImageAsync(int productId,int imageId,CancellationToken cancellationToken = default); 
    }
}
