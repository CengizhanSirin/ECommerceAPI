using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.ProductImage.Requests;
using ECommerce.Application.DTOs.ProductImage.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductImageService(IProductImageRepository productImageRepository, IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultT<ProductImageResponse>> CreateAsync(int productId, CreateProductImageRequest request, CancellationToken cancellationToken = default)
        {
            var productExists = await _productRepository.AnyAsync(x => x.Id == productId, cancellationToken);

            if (!productExists)
                return ResultT<ProductImageResponse>.NotFound(ProductImageMessages.ProductNotFound);

            if (request.IsMain)
            {
                var productImages = await _productImageRepository.WhereAsync(x => x.ProductId == productId, cancellationToken);

                foreach (var image in productImages)
                {
                    image.IsMain = false;
                }
            }

            var productImage = _mapper.Map<ProductImage>(request);
            productImage.ProductId = productId;

            await _productImageRepository.AddAsync(productImage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<ProductImageResponse>(productImage);

            return ResultT<ProductImageResponse>.Success(response, ProductImageMessages.ProductImageCreated);
        }

        public async Task<Result> DeleteAsync(int productId, int imageId, CancellationToken cancellationToken = default)
        {
            var image = await _productImageRepository.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, cancellationToken);

            if (image is null)
                return Result.NotFound(ProductImageMessages.ProductImageNotFound);

            _productImageRepository.Delete(image);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ProductImageMessages.ProductImageDeleted);
        }

        public async Task<ResultT<IReadOnlyList<ProductImageResponse>>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var productExists = await _productRepository.AnyAsync(x => x.Id == productId, cancellationToken);

            if (!productExists)
                return ResultT<IReadOnlyList<ProductImageResponse>>.NotFound(ProductImageMessages.ProductNotFound);


            var images = await _productImageRepository.WhereAsync(x => x.ProductId == productId, cancellationToken);

            var orderedImages = images.OrderBy(x => x.DisplayOrder).ToList();

            var response = _mapper.Map<IReadOnlyList<ProductImageResponse>>(orderedImages);

            return ResultT<IReadOnlyList<ProductImageResponse>>.Success(response, ProductImageMessages.ProductImagesRetrieved);
        }

        public async Task<Result> SetMainImageAsync(int productId, int imageId, CancellationToken cancellationToken = default)
        {
            var image = await _productImageRepository.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, cancellationToken);

            if (image is null)
                return Result.NotFound(ProductImageMessages.ProductImageNotFound);

            var productImages = await _productImageRepository.WhereAsync(x => x.ProductId == productId, cancellationToken);

            foreach (var productImage in productImages)
                productImage.IsMain = productImage.Id == imageId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ProductImageMessages.MainImageUpdated);
        }

        public async Task<Result> UpdateAsync(int productId, int imageId, UpdateProductImageRequest request, CancellationToken cancellationToken = default)
        {
            var image = await _productImageRepository.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, cancellationToken);

            if (image is null)
                return Result.NotFound(ProductImageMessages.ProductImageNotFound);

            if (request.IsMain)
            {
                var productImages = await _productImageRepository.WhereAsync(x => x.ProductId == productId && x.Id != imageId, cancellationToken);
                foreach (var productImage in productImages)
                    productImage.IsMain = false;
            }

            _mapper.Map(request, image);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ProductImageMessages.ProductImageUpdated);
        }
    }
}
