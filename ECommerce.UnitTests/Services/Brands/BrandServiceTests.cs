using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Brand.Requests;
using ECommerce.Application.DTOs.Brand.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Brands
{
    public sealed class BrandServiceTests
    {
        private readonly Mock<IBrandRepository> _brandRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly BrandService _brandService;

        public BrandServiceTests()
        {
            _brandRepositoryMock = new Mock<IBrandRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _brandService = new BrandService(_brandRepositoryMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnConflict_When_BrandNameAlreadyExists()
        {
            // Arrange
            var request = new CreateBrandRequest("Apple", "Apple Ürünleri", "apple.jpg");

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _brandService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandNameAlreadyExists, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<Brand>(It.IsAny<CreateBrandRequest>()), Times.Never);

            _brandRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_CreateBrand_When_BrandNameDoesNotExist()
        {
            // Arrange
            var request = new CreateBrandRequest("Apple", "Apple Ürünleri", "apple.jpg");
            var brand = new Brand { Name = request.Name, Description = request.Description, LogoUrl = request.LogoUrl };
            var brandresponse = new BrandResponse { Name = brand.Name, Description = brand.Description, LogoUrl = brand.LogoUrl };

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(mapper => mapper.Map<Brand>(It.IsAny<CreateBrandRequest>()))
                .Returns(brand);

            _mapperMock.Setup(mapper => mapper.Map<BrandResponse>(It.IsAny<Brand>()))
                .Returns(brandresponse);

            // Act
            var result = await _brandService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandCreated, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(brandresponse, result.Data);

            _brandRepositoryMock.Verify(repository => repository.AddAsync(brand, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<BrandResponse>(brand), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnNotFound_When_BrandDoesNotExist()
        {
            // Arrange
            const int brandId = 999;

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
               .ReturnsAsync((Brand?)null);

            // Act
            var result = await _brandService.DeleteAsync(brandId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandNotFound, result.Message);

            _brandRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Brand>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _brandRepositoryMock.Verify(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteBrand_When_BrandExists()
        {
            // Arrange
            const int brandId = 1;
            var brand = new Brand
            {
                Id = brandId,
                Name = "Apple",
                Description = "Apple Ürünleri",
                LogoUrl = "apple.jpg"
            };

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            // Act
            var result = await _brandService.DeleteAsync(brandId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandDeleted, result.Message);

            _brandRepositoryMock.Verify(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once());

            _brandRepositoryMock.Verify(repository => repository.Delete(brand), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());


        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNotFound_When_BrandDoesNotExist()
        {
            // Arrange
            const int brandId = 999;

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync((Brand?)null);

            // Act
            var result = await _brandService.GetByIdAsync(brandId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandNotFound, result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<BrandDetailResponse>(It.IsAny<Brand>()), Times.Never());

            _brandRepositoryMock.Verify(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnBrand_When_BrandExists()
        {
            // Arrange
            const int brandId = 1;

            var brand = new Brand
            {
                Id = brandId,
                Name = "Apple",
                Description = "Apple ürünleri",
                LogoUrl = "apple.jpg"
            };

            var brandResponse = new BrandDetailResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl
            };

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            _mapperMock.Setup(mapper => mapper.Map<BrandDetailResponse>(brand)).Returns(brandResponse);

            // Act
            var result = await _brandService.GetByIdAsync(brandId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandRetrieved, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(brandResponse, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<BrandDetailResponse>(brand), Times.Once());

            _brandRepositoryMock.Verify(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_BrandDoesNotExist()
        {
            // Arrange
            const int brandId = 999;

            var request = new UpdateBrandRequest("Apple Güncel", "Güncel açıklama", "apple-updated.jpg", true);

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync((Brand?)null);

            // Act
            var result = await _brandService.UpdateAsync(brandId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandNotFound, result.Message);

            _brandRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateBrandRequest, Brand>(It.IsAny<UpdateBrandRequest>(), It.IsAny<Brand>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnConflict_When_BrandNameAlreadyExists()
        {
            // Arrange
            const int brandId = 1;

            var brand = new Brand
            {
                Id = brandId,
                Name = "Apple",
                Description = "Apple ürünleri",
                LogoUrl = "apple.jpg"
            };

            var request = new UpdateBrandRequest("Samsung", "Güncel açıklama", "samsung.jpg", true);

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _brandService.UpdateAsync(brandId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandNameAlreadyExists, result.Message);


            _mapperMock.Verify(mapper => mapper.Map<UpdateBrandRequest, Brand>(It.IsAny<UpdateBrandRequest>(), It.IsAny<Brand>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateBrand_When_RequestIsValid()
        {
            // Arrange
            const int brandId = 1;

            var brand = new Brand
            {
                Id = brandId,
                Name = "Apple",
                Description = "Eski açıklama",
                LogoUrl = "old-apple.jpg",
                IsActive = true
            };

            var request = new UpdateBrandRequest("Apple Güncel", "Güncel açıklama", "apple-updated.jpg", false);

            _brandRepositoryMock.Setup(repository => repository.GetByIdAsync(brandId, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            _brandRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mapperMock.Setup(mapper => mapper.Map<UpdateBrandRequest, Brand>(request, brand)).Returns(brand);

            // Act
            var result = await _brandService.UpdateAsync(brandId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandUpdated, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateBrandRequest, Brand>(request, brand), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        }

        [Fact]
        public async Task GetAllAsync_Should_ReturnPagedBrandList()
        {
            // Arrange
            var request = new PaginationRequest(PageNumber: 2, PageSize: 2);

            var brands = new List<Brand>
            {
                new()
                {
                    Id = 3,
                    Name = "Apple",
                    Description = "Apple ürünleri",
                    LogoUrl = "apple.jpg",
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    Name = "Samsung",
                    Description = "Samsung ürünleri",
                    LogoUrl = "samsung.jpg",
                    IsActive = true
                }
            };

            var brandResponses = new List<BrandListResponse>
            {
                new()
                {
                    Id = 3,
                    Name = "Apple",
                    LogoUrl = "apple.jpg",
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    Name = "Samsung",
                    LogoUrl = "samsung.jpg",
                    IsActive = true
                }
            };

            const int totalCount = 5;

            _brandRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Brand, bool>>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(totalCount);

            _brandRepositoryMock.Setup(repository => repository.GetPagedAsync(
                    It.IsAny<Expression<Func<Brand, bool>>?>(),
                    It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),
                    request.Skip,
                    request.PageSize,
                    It.IsAny<CancellationToken>())).ReturnsAsync(brands);


            _mapperMock.Setup(mapper => mapper.Map<List<BrandListResponse>>(brands)).Returns(brandResponses);

            // Act
            var result = await _brandService.GetAllAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BrandMessages.BrandsRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(brandResponses, result.Data.Items);

            Assert.Equal(totalCount, result.Data.TotalCount);
            Assert.Equal(request.PageNumber, result.Data.PageNumber);
            Assert.Equal(request.PageSize, result.Data.PageSize);
            Assert.Equal(3, result.Data.TotalPages);

            Assert.True(result.Data.HasPreviousPage);
            Assert.True(result.Data.HasNextPage);

            _brandRepositoryMock.Verify(repository => repository.CountAsync(It.IsAny<Expression<Func<Brand, bool>>?>(), It.IsAny<CancellationToken>()), Times.Once());

            _brandRepositoryMock.Verify(repository => repository.GetPagedAsync(It.IsAny<Expression<Func<Brand, bool>>?>(), It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),
                    request.Skip, request.PageSize, It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<List<BrandListResponse>>(brands), Times.Once());
        }
    }
}
