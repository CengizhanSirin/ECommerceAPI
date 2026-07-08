using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.Product.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultT<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var skuExists = await _productRepository.AnyAsync(x => x.SKU == request.SKU, cancellationToken);

            if (skuExists)
                return ResultT<ProductResponse>.Conflict(ProductMessages.ProductSkuAlreadyExists);

            var categoryExists = await _categoryRepository.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
                return ResultT<ProductResponse>.NotFound(ProductMessages.ProductCategoryNotFound);

            var brandExists = await _brandRepository.AnyAsync(x => x.Id == request.BrandId, cancellationToken);

            if (!brandExists)
                return ResultT<ProductResponse>.NotFound(ProductMessages.ProductBrandNotFound);

            var product = _mapper.Map<Product>(request);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdProduct = await _productRepository.GetByIdWithRelationsAsync(product.Id,  cancellationToken);

            if (createdProduct is null)
                return ResultT<ProductResponse>.NotFound(ProductMessages.ProductNotFound);

            var response = _mapper.Map<ProductResponse>(createdProduct);

            return ResultT<ProductResponse>.Success(response, ProductMessages.ProductCreated);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);

            if (product is null)
                return Result.NotFound(ProductMessages.ProductNotFound);

            _productRepository.Delete(product);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ProductMessages.ProductDeleted);
        }

        public async Task<ResultT<PagedResult<ProductListResponse>>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var totalCount = await _productRepository.CountAsync(cancellationToken: cancellationToken);

            var products = await _productRepository.GetPagedWithRelationsAsync(request.Skip, request.PageSize, cancellationToken);

            var items = _mapper.Map<List<ProductListResponse>>(products);

            var pagedResult = PagedResult<ProductListResponse>.Create(items, totalCount, request.PageNumber, request.PageSize);

            return ResultT<PagedResult<ProductListResponse>>.Success(pagedResult, ProductMessages.ProductsRetrieved);
        }

        public async Task<ResultT<ProductDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdWithRelationsAsync(id, cancellationToken);

            if (product is null)
                return ResultT<ProductDetailResponse>.NotFound(ProductMessages.ProductNotFound);

            var response = _mapper.Map<ProductDetailResponse>(product);

            return ResultT<ProductDetailResponse>.Success(response, ProductMessages.ProductRetrieved);
        }

        public async Task<Result> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);

            if (product is null)
                return Result.NotFound(ProductMessages.ProductNotFound);

            var categoryExists = await _categoryRepository.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
                return Result.NotFound(ProductMessages.ProductCategoryNotFound);

            var brandExists = await _brandRepository.AnyAsync(x => x.Id == request.BrandId, cancellationToken);

            if (!brandExists)
                return Result.NotFound(ProductMessages.ProductBrandNotFound);

            _mapper.Map(request, product);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ProductMessages.ProductUpdated);
        }
    }
}
