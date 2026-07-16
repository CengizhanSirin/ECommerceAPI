using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.Product.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Products
{
    public sealed class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IBrandRepository> _brandRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _brandRepositoryMock = new Mock<IBrandRepository>();
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _productService = new ProductService(_productRepositoryMock.Object, _categoryRepositoryMock.Object, _brandRepositoryMock.Object,
                _mapperMock.Object, _unitOfWorkMock.Object);
        }

        // =========================================================
        // CREATE
        // =========================================================


        [Fact]
        public async Task CreateAsync_Should_ReturnConflict_When_SkuAlreadyExists()
        {
            // Arrange
            var request = CreateValidRequest();

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _productService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductSkuAlreadyExists, result.Message);
            Assert.Null(result.Data);

            _categoryRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _brandRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<Product>(It.IsAny<CreateProductRequest>()), Times.Never());

            _productRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            var request = CreateValidRequest();

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductCategoryNotFound, result.Message);
            Assert.Null(result.Data);

            _brandRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<Product>(It.IsAny<CreateProductRequest>()), Times.Never());

            _productRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_BrandDoesNotExist()
        {
            // Arrange
            var request = CreateValidRequest();

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductBrandNotFound, result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<Product>(It.IsAny<CreateProductRequest>()), Times.Never());

            _productRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_CreatedProductCannotBeReloaded()
        {
            // Arrange
            var request = CreateValidRequest();
            var product = CreateProduct();

            // Mock SaveChanges ID üretmediği için açıkça belirliyoruz.
            product.Id = 10;

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _mapperMock.Setup(mapper => mapper.Map<Product>(request)).Returns(product);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _productService.CreateAsync(request));

            // Assert
            Assert.Contains($"ProductId: {product.Id}", exception.Message);


            _productRepositoryMock.Verify(repository => repository.AddAsync(product, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _productRepositoryMock.Verify(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<ProductResponse>(It.IsAny<Product>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_CreateProduct_When_RequestIsValid()
        {
            // Arrange
            var request = CreateValidRequest();
            var product = CreateProduct();
            var createdProduct = CreateProduct();
            var response = CreateProductResponse(createdProduct);

            product.Id = 10;
            createdProduct.Id = product.Id;

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _mapperMock.Setup(mapper => mapper.Map<Product>(request)).Returns(product);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(createdProduct);

            _mapperMock.Setup(mapper => mapper.Map<ProductResponse>(createdProduct)).Returns(response);

            // Act
            var result = await _productService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductCreated, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);


            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _productRepositoryMock.Verify(repository => repository.AddAsync(product, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _productRepositoryMock.Verify(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<ProductResponse>(createdProduct), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }


        // =========================================================
        // DELETE
        // =========================================================

        [Fact]
        public async Task DeleteAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            const int productId = 999;

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.DeleteAsync(productId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductNotFound, result.Message);

            _productRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Product>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteProduct_When_ProductExists()
        {
            // Arrange
            const int productId = 1;
            var product = CreateProduct(productId);

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductDeleted, result.Message);

            _productRepositoryMock.Verify(repository => repository.Delete(product), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // GET ALL
        // =========================================================

        [Fact]
        public async Task GetAllAsync_Should_ReturnPagedProductList()
        {
            // Arrange
            var request = new PaginationRequest(
                PageNumber: 2,
                PageSize: 2);

            var products = new List<Product>
            {
                new()
                {
                    Id = 3,
                    Name = "iPhone 15",
                    SKU = "APL-IP15",
                    Price = 50_000m,
                    Stock = 10,
                    CategoryId = 2,
                    BrandId = 1
                },
                new()
                {
                    Id = 4,
                    Name = "Galaxy S24",
                    SKU = "SMS-S24",
                    Price = 45_000m,
                    Stock = 15,
                    CategoryId = 2,
                    BrandId = 2
                }
            };


            var responses = new List<ProductListResponse>
            {
                new()
                {
                    Id = 3,
                    Name = "iPhone 15",
                    Price = 50_000m,
                    Stock = 10,
                    IsActive = true,
                    MainImageUrl = "iphone.jpg",
                    CategoryName = "Telefon",
                    BrandName = "Apple"
                },
                new()
                {
                    Id = 4,
                    Name = "Galaxy S24",
                    Price = 45_000m,
                    Stock = 15,
                    IsActive = true,
                    MainImageUrl = "galaxy.jpg",
                    CategoryName = "Telefon",
                    BrandName = "Samsung"
                }
            };

            const int totalCount = 5;

            _productRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Product, bool>>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(totalCount);

            _productRepositoryMock.Setup(repository => repository.GetPagedWithRelationsAsync(request.Skip, request.PageSize, It.IsAny<CancellationToken>())).ReturnsAsync(products);

            _mapperMock.Setup(mapper => mapper.Map<List<ProductListResponse>>(products)).Returns(responses);

            // Act
            var result = await _productService.GetAllAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductsRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(responses, result.Data.Items);

            Assert.Equal(totalCount, result.Data.TotalCount);
            Assert.Equal(request.PageNumber, result.Data.PageNumber);
            Assert.Equal(request.PageSize, result.Data.PageSize);
            Assert.Equal(3, result.Data.TotalPages);

            Assert.True(result.Data.HasPreviousPage);
            Assert.True(result.Data.HasNextPage);

            _productRepositoryMock.Verify(repository => repository.CountAsync(It.IsAny<Expression<Func<Product, bool>>?>(), It.IsAny<CancellationToken>()), Times.Once());

            _productRepositoryMock.Verify(repository => repository.GetPagedWithRelationsAsync(request.Skip, request.PageSize, It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<List<ProductListResponse>>(products), Times.Once());

        }

        // =========================================================
        // GET BY ID
        // =========================================================

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            const int productId = 999;

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductNotFound, result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<ProductDetailResponse>(It.IsAny<Product>()), Times.Never());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnProduct_When_ProductExists()
        {
            // Arrange
            const int productId = 1;

            var product = CreateProduct(productId);
            var response = CreateProductDetailResponse(product);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _mapperMock.Setup(mapper => mapper.Map<ProductDetailResponse>(product)).Returns(response);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductRetrieved, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<ProductDetailResponse>(product), Times.Once());
        }

        // =========================================================
        // UPDATE
        // =========================================================

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            const int productId = 999;
            var request = CreateValidUpdateRequest();

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.UpdateAsync(productId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductNotFound, result.Message);

            _categoryRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _brandRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductRequest, Product>(It.IsAny<UpdateProductRequest>(), It.IsAny<Product>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            const int productId = 1;

            var product = CreateProduct(productId);
            var request = CreateValidUpdateRequest();

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productService.UpdateAsync(productId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductCategoryNotFound, result.Message);

            _brandRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductRequest, Product>(It.IsAny<UpdateProductRequest>(), It.IsAny<Product>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_BrandDoesNotExist()
        {
            // Arrange
            const int productId = 1;

            var product = CreateProduct(productId);
            var request = CreateValidUpdateRequest();

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productService.UpdateAsync(
                productId,
                request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductBrandNotFound, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductRequest, Product>(It.IsAny<UpdateProductRequest>(), It.IsAny<Product>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateProduct_When_RequestIsValid()
        {
            // Arrange
            const int productId = 1;

            var product = CreateProduct(productId);
            var request = CreateValidUpdateRequest();

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _mapperMock.Setup(mapper => mapper.Map<UpdateProductRequest, Product>(request, product)).Returns(product);

            // Act
            var result = await _productService.UpdateAsync(productId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductMessages.ProductUpdated, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductRequest, Product>(request, product), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // TEST DATA HELPERS
        // =========================================================

        private static CreateProductRequest CreateValidRequest()
        {
            return new CreateProductRequest(
                Name: "iPhone 15",
                Description: "Apple akıllı telefon",
                SKU: "APL-IP15",
                Price: 50_000m,
                Stock: 10,
                BrandId: 1,
                CategoryId: 2);
        }

        private static UpdateProductRequest CreateValidUpdateRequest()
        {
            return new UpdateProductRequest(
                Name: "iPhone 15 Updated",
                Description: "Güncellenmiş açıklama",
                Price: 48_000m,
                Stock: 20,
                IsActive: true,
                BrandId: 1,
                CategoryId: 2);
        }

        private static Product CreateProduct(int id = 1)
        {
            return new Product
            {
                Id = id,
                Name = "iPhone 15",
                Description = "Apple akıllı telefon",
                SKU = "APL-IP15",
                Price = 50_000m,
                Stock = 10,
                IsActive = true,
                BrandId = 1,
                CategoryId = 2
            };
        }

        private static ProductResponse CreateProductResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = "Telefon",
                BrandId = product.BrandId,
                BrandName = "Apple"
            };
        }

        private static ProductDetailResponse CreateProductDetailResponse(Product product)
        {
            return new ProductDetailResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = "Telefon",
                BrandId = product.BrandId,
                BrandName = "Apple",
                Images = ["iphone-front.jpg", "iphone-back.jpg"],
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };
        }
    }
}
