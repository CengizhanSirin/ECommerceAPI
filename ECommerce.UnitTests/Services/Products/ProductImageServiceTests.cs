using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.ProductImage.Requests;
using ECommerce.Application.DTOs.ProductImage.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Products
{
    public sealed class ProductImageServiceTests
    {
        private readonly Mock<IProductImageRepository> _productImageRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly ProductImageService _productImageService;

        public ProductImageServiceTests()
        {
            _productImageRepositoryMock = new Mock<IProductImageRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _productImageService = new ProductImageService(_productImageRepositoryMock.Object, _productRepositoryMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            const int productId = 999;
            var request = CreateValidCreateRequest();

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productImageService.CreateAsync(productId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductNotFound, result.Message);
            Assert.Null(result.Data);

            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<ProductImage>(It.IsAny<CreateProductImageRequest>()), Times.Never());

            _productImageRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<ProductImage>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ClearExistingMainImages_When_NewImageIsMain()
        {
            // Arrange
            const int productId = 1;

            var request = CreateValidCreateRequest(isMain: true);

            var existingImages = new List<ProductImage>
            {
            CreateProductImage(  id: 1, productId: productId, isMain: true),
            CreateProductImage(  id: 2,productId: productId, isMain: false)
            };

            var productImage = CreateProductImage(id: 3, productId: 0, isMain: true);

            var response = CreateProductImageResponse(productImage);

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _productImageRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingImages);

            _mapperMock.Setup(mapper => mapper.Map<ProductImage>(request)).Returns(productImage);

            _mapperMock.Setup(mapper => mapper.Map<ProductImageResponse>(productImage)).Returns(response);


            // Act
            var result = await _productImageService.CreateAsync(productId, request);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageCreated, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);
            Assert.Equal(productId, productImage.ProductId);
            Assert.All(existingImages, image => Assert.False(image.IsMain));


            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Once());

            _productImageRepositoryMock.Verify(repository => repository.AddAsync(productImage, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_Should_CreateImageWithoutLoadingExistingImages_When_ImageIsNotMain()
        {
            // Arrange
            const int productId = 1;

            var request = CreateValidCreateRequest(isMain: false);

            var productImage = CreateProductImage(id: 1, productId: 0, isMain: false);

            var response = CreateProductImageResponse(productImage);

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _mapperMock.Setup(mapper => mapper.Map<ProductImage>(request)).Returns(productImage);

            _mapperMock.Setup(mapper => mapper.Map<ProductImageResponse>(productImage)).Returns(response);

            // Act
            var result = await _productImageService.CreateAsync(productId, request);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageCreated, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);
            Assert.Equal(productId, productImage.ProductId);

            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _productImageRepositoryMock.Verify(repository => repository.AddAsync(productImage, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // DELETE
        // =========================================================

        [Fact]
        public async Task DeleteAsync_Should_ReturnNotFound_When_ImageDoesNotExist()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 999;

            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductImage?)null);

            // Act
            var result = await _productImageService.DeleteAsync(productId, imageId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageNotFound, result.Message);

            _productImageRepositoryMock.Verify(repository => repository.Delete(It.IsAny<ProductImage>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteImage_When_ImageExists()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 10;

            var image = CreateProductImage(id: imageId, productId: productId);

            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(image);

            // Act
            var result = await _productImageService.DeleteAsync(productId, imageId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageDeleted, result.Message);

            _productImageRepositoryMock.Verify(repository => repository.Delete(image), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // GET BY PRODUCT ID
        // =========================================================

        [Fact]
        public async Task GetByProductIdAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            const int productId = 999;

            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _productImageService.GetByProductIdAsync(productId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductNotFound, result.Message);
            Assert.Null(result.Data);

            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<IReadOnlyList<ProductImageResponse>>(It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task GetByProductIdAsync_Should_ReturnImagesOrderedByDisplayOrder()
        {
            // Arrange
            const int productId = 1;

            var images = new List<ProductImage>
            {

            CreateProductImage(id: 1, productId: productId,displayOrder: 3),
            CreateProductImage(id: 2, productId: productId,displayOrder: 1),
            CreateProductImage(id: 3, productId: productId,displayOrder: 2),
            };

            IReadOnlyList<ProductImageResponse> responses = new List<ProductImageResponse>
            {
                CreateProductImageResponse(images[1]),
                CreateProductImageResponse(images[2]),
                CreateProductImageResponse(images[0])
            };


            _productRepositoryMock.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            _productImageRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(images);

            _mapperMock.Setup(mapper => mapper.Map<IReadOnlyList<ProductImageResponse>>(
                It.Is<List<ProductImage>>(orderedImages => orderedImages.Select(image => image.Id).SequenceEqual(new[] { 2, 3, 1 })))).Returns(responses);

            // Act
            var result =
                await _productImageService.GetByProductIdAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImagesRetrieved, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(responses, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<IReadOnlyList<ProductImageResponse>>(
                  It.Is<List<ProductImage>>(orderedImages => orderedImages.Select(image => image.Id).SequenceEqual(new[] { 2, 3, 1 }))), Times.Once());
        }

        // =========================================================
        // SET MAIN IMAGE
        // =========================================================

        [Fact]
        public async Task SetMainImageAsync_Should_ReturnNotFound_When_ImageDoesNotExist()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 999;

            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductImage?)null);

            // Act
            var result = await _productImageService.SetMainImageAsync(productId, imageId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageNotFound, result.Message);

            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task SetMainImageAsync_Should_SetSelectedImageAsMain()
        {
            // Arrange
            const int productId = 1;
            const int selectedImageId = 2;

            var firstImage = CreateProductImage(id: 1, productId: productId, isMain: true);

            var selectedImage = CreateProductImage(id: selectedImageId, productId: productId, isMain: false);

            var thirdImage = CreateProductImage(id: 3, productId: productId, isMain: false);

            var productImages = new List<ProductImage>
            {
                firstImage,
                selectedImage,
                thirdImage
            };


            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(selectedImage);

            _productImageRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(productImages);

            // Act
            var result = await _productImageService.SetMainImageAsync(productId, selectedImageId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.MainImageUpdated, result.Message);

            Assert.False(firstImage.IsMain);
            Assert.True(selectedImage.IsMain);
            Assert.False(thirdImage.IsMain);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // UPDATE
        // =========================================================

        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_ImageDoesNotExist()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 999;

            var request = CreateValidUpdateRequest();

            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductImage?)null);

            // Act
            var result = await _productImageService.UpdateAsync(productId, imageId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageNotFound, result.Message);


            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductImageRequest, ProductImage>(It.IsAny<UpdateProductImageRequest>(), It.IsAny<ProductImage>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_ClearOtherMainImages_When_UpdatedImageIsMain()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 2;

            var request = CreateValidUpdateRequest(isMain: true);

            var image = CreateProductImage(id: imageId, productId: productId, isMain: false);

            var otherImages = new List<ProductImage>
            {

            CreateProductImage( id: 1, productId: productId,isMain: true),
            CreateProductImage( id: 3, productId: productId,isMain: false),
            };


            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(image);

            _productImageRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(otherImages);

            _mapperMock.Setup(mapper => mapper.Map<UpdateProductImageRequest, ProductImage>(request, image)).Returns(image);

            // Act
            var result = await _productImageService.UpdateAsync(productId, imageId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageUpdated, result.Message);
            Assert.All(otherImages, otherImage => Assert.False(otherImage.IsMain));

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductImageRequest, ProductImage>(request, image), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateImageWithoutLoadingOtherImages_When_ImageIsNotMain()
        {
            // Arrange
            const int productId = 1;
            const int imageId = 2;

            var request = CreateValidUpdateRequest(isMain: false);

            var image = CreateProductImage(id: imageId, productId: productId, isMain: true);

            _productImageRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(image);

            _mapperMock.Setup(mapper => mapper.Map<UpdateProductImageRequest, ProductImage>(request, image)).Returns(image);

            // Act
            var result = await _productImageService.UpdateAsync(productId, imageId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ProductImageMessages.ProductImageUpdated, result.Message);

            _productImageRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<ProductImage, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<UpdateProductImageRequest, ProductImage>(request, image), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // TEST DATA HELPERS
        // =========================================================

        private static CreateProductImageRequest CreateValidCreateRequest(bool isMain = false)
        {
            return new CreateProductImageRequest(ImageUrl: "product-image.jpg", IsMain: isMain, DisplayOrder: 1);
        }

        private static UpdateProductImageRequest CreateValidUpdateRequest(bool isMain = false)
        {
            return new UpdateProductImageRequest(ImageUrl: "updated-product-image.jpg", IsMain: isMain, DisplayOrder: 2);
        }

        private static ProductImage CreateProductImage(int id = 1, int productId = 1, bool isMain = false, int displayOrder = 1)
        {
            return new ProductImage
            {
                Id = id,
                ProductId = productId,
                ImageUrl = $"image-{id}.jpg",
                IsMain = isMain,
                DisplayOrder = displayOrder
            };
        }

        private static ProductImageResponse CreateProductImageResponse(ProductImage image)
        {
            return new ProductImageResponse
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                IsMain = image.IsMain,
                DisplayOrder = image.DisplayOrder
            };
        }
    }
}

