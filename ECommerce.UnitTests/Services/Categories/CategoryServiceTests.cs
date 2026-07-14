using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Category.Requests;
using ECommerce.Application.DTOs.Category.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Categories
{
    public sealed class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _categoryService = new CategoryService(_categoryRepositoryMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnConflict_When_CategoryNameAlreadyExists()
        {
            // Arrange
            var request = new CreateCategoryRequest(
                Name: "Elektronik",
                Description: "Elektronik ürünler",
                ImageUrl: "electronics.jpg",
                DisplayOrder: 1);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _categoryService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryNameAlreadyExists, result.Message);


            _mapperMock.Verify(mapper => mapper.Map<Category>(It.IsAny<CreateCategoryRequest>()), Times.Never);

            _categoryRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Category>()), Times.Never);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_CreateCategory_When_CategoryNameDoesNotExist()
        {
            // Arrange
            var request = new CreateCategoryRequest(
                Name: "Elektronik",
                Description: "Elektronik ürünler",
                ImageUrl: "electronics.jpg",
                DisplayOrder: 1);

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder
            };

            var categoryResponse = new CategoryResponse
            {
                Id = 1,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };


            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(mapper => mapper.Map<Category>(request)).Returns(category);

            _mapperMock.Setup(mapper => mapper.Map<CategoryResponse>(category)).Returns(categoryResponse);

            // Act
            var result = await _categoryService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryCreated, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(categoryResponse, result.Data);

            _categoryRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Category>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            const int categoryId = 999;

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryNotFound, result.Message);


            _categoryRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Category>()), Times.Never);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            _categoryRepositoryMock.Verify(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteCategory_When_CategoryExists()
        {
            // Arrange
            const int categoryId = 1;

            var category = new Category
            {
                Id = categoryId,
                Name = "Elektronik",
                DisplayOrder = 1
            };

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryDeleted, result.Message);


            _categoryRepositoryMock.Verify(repository => repository.Delete(category), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            const int categoryId = 999;

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryNotFound, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<CategoryDetailResponse>(It.IsAny<Category>()), Times.Never());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnCategory_When_CategoryExists()
        {
            // Arrange
            const int categoryId = 1;

            var category = new Category
            {
                Id = categoryId,
                Name = "Elektronik",
                Description = "Elektronik ürünler",
                ImageUrl = "electronics.jpg",
                DisplayOrder = 1,
                IsActive = true
            };

            var categoryResponse = new CategoryDetailResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            _mapperMock.Setup(mapper => mapper.Map<CategoryDetailResponse>(category)).Returns(categoryResponse);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryRetrieved, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(categoryResponse, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<CategoryDetailResponse>(category), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            const int categoryId = 999;

            var request = new UpdateCategoryRequest(
                Name: "Yeni Elektronik",
                Description: "Güncel yeni açıklama",
                ImageUrl: "updated.jpg",
                DisplayOrder: 2,
                IsActive: true);

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.UpdateAsync(categoryId, request);


            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryNotFound, result.Message);

            _categoryRepositoryMock.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateCategoryRequest, Category>(It.IsAny<UpdateCategoryRequest>(), It.IsAny<Category>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnConflict_When_CategoryNameAlreadyExists()
        {
            // Arrange
            const int categoryId = 1;

            var category = new Category
            {
                Id = categoryId,
                Name = "Elektronik",
                DisplayOrder = 1
            };

            var request = new UpdateCategoryRequest(
                Name: "Bilgisayar",
                Description: "Bilgisayar ürünleri",
                ImageUrl: "computer.jpg",
                DisplayOrder: 2,
                IsActive: true);

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _categoryService.UpdateAsync(categoryId, request);


            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryNameAlreadyExists, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateCategoryRequest, Category>(It.IsAny<UpdateCategoryRequest>(), It.IsAny<Category>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateCategory_When_RequestIsValid()
        {
            // Arrange
            const int categoryId = 1;

            var category = new Category
            {
                Id = categoryId,
                Name = "Elektronik",
                Description = "Eski açıklama",
                ImageUrl = "old.jpg",
                DisplayOrder = 1,
                IsActive = true
            };

            var request = new UpdateCategoryRequest(
                Name: "Yeni Elektronik",
                Description: "Güncellenen yeni açıklama",
                ImageUrl: "updated.jpg",
                DisplayOrder: 2,
                IsActive: false);

            _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            _categoryRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mapperMock.Setup(mapper => mapper.Map<UpdateCategoryRequest, Category>(request, category)).Returns(category);


            // Act
            var result = await _categoryService.UpdateAsync(categoryId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryUpdated, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateCategoryRequest, Category>(request, category), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetAllAsync_Should_ReturnPagedCategoryList()
        {
            // Arrange
            var request = new PaginationRequest(PageNumber: 2, PageSize: 2);
            const int totalCount = 5;

            var categories = new List<Category>
            {
                new()
                {
                    Id = 3,
                    Name = "Elektronik",
                    ImageUrl = "electronics.jpg",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    Name = "Kitap",
                    ImageUrl = "books.jpg",
                    DisplayOrder = 4,
                    IsActive = true
                }
            };

            var categoryResponses = new List<CategoryListResponse>
            {
                new()
                {
                    Id = 3,
                    Name = "Elektronik",
                    ImageUrl = "electronics.jpg",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    Name = "Kitap",
                    ImageUrl = "books.jpg",
                    DisplayOrder = 4,
                    IsActive = true
                }
            };


            _categoryRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Category, bool>>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalCount);


            _categoryRepositoryMock.Setup(repository => repository.GetPagedAsync(It.IsAny<Expression<Func<Category, bool>>?>(), It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>(),
                    request.Skip, request.PageSize, It.IsAny<CancellationToken>())).ReturnsAsync(categories);


            _mapperMock.Setup(mapper => mapper.Map<List<CategoryListResponse>>(categories)).Returns(categoryResponses);


            // Act
            var result = await _categoryService.GetAllAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CategoryMessages.CategoryListRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(categoryResponses, result.Data.Items);

            Assert.Equal(totalCount, result.Data.TotalCount);
            Assert.Equal(request.PageNumber, result.Data.PageNumber);
            Assert.Equal(request.PageSize, result.Data.PageSize);
            Assert.Equal(3, result.Data.TotalPages);

            Assert.True(result.Data.HasPreviousPage);
            Assert.True(result.Data.HasNextPage);

            _categoryRepositoryMock.Verify(repository => repository.CountAsync(It.IsAny<Expression<Func<Category, bool>>?>(), It.IsAny<CancellationToken>()), Times.Once());

            _categoryRepositoryMock.Verify(
                repository => repository.GetPagedAsync(It.IsAny<Expression<Func<Category, bool>>?>(), It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>(),
                request.Skip, request.PageSize, It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<List<CategoryListResponse>>(categories), Times.Once());
        }
    }
}
