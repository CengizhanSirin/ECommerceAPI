using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Cart.Requests;
using ECommerce.Application.DTOs.Cart.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;

namespace ECommerce.UnitTests.Services.Carts
{
    public sealed class CartServiceTests
    {
        private const int CurrentUserId = 1281;

        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _cartItemRepositoryMock = new Mock<ICartItemRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _currentUserServiceMock.SetupGet(service => service.UserId).Returns(CurrentUserId);

            _cartService = new CartService(_cartRepositoryMock.Object, _cartItemRepositoryMock.Object, _productRepositoryMock.Object, _currentUserServiceMock.Object,
                _unitOfWorkMock.Object, _mapperMock.Object);
        }

        // =========================================================
        // ADD ITEM
        // =========================================================

        [Fact]
        public async Task AddItemAsync_Should_ReturnNotFound_When_ProductDoesNotExist()
        {
            // Arrange
            var request = new AddToCartRequest(ProductId: 10, Quantity: 2);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(request.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.ProductNotFound, result.Message);
            Assert.Null(result.Data);

            _cartRepositoryMock.Verify(repository => repository.GetWithItemsByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _cartItemRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddItemAsync_Should_ReturnBadRequest_When_ProductIsNotActive()
        {
            // Arrange
            var product = CreateProduct(isActive: false);

            var request = new AddToCartRequest(ProductId: product.Id, Quantity: 2);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(request.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.ProductNotActive, result.Message);
            Assert.Null(result.Data);

            _cartRepositoryMock.Verify(repository => repository.GetWithItemsByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddItemAsync_Should_ReturnBadRequest_When_NewQuantityExceedsStock()
        {
            // Arrange
            var product = CreateProduct(stock: 5, isActive: true);

            var existingItem = CreateCartItem(product: product, quantity: 4);

            var cart = CreateCart(items: [existingItem]);


            var request = new AddToCartRequest(ProductId: product.Id, Quantity: 2);

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(request.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.InsufficientStock, result.Message);
            Assert.Equal(4, existingItem.Quantity);

            _cartItemRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddItemAsync_Should_CreateCartAndAddItem_When_CartDoesNotExist()
        {
            // Arrange
            const int generatedCartId = 100;

            var product = CreateProduct(id: 10, price: 250m, stock: 10);

            var request = new AddToCartRequest(ProductId: product.Id, Quantity: 2);

            var updatedCartItem = CreateCartItem(id: 1, cartId: generatedCartId, product: product, quantity: request.Quantity);

            var updatedCart = CreateCart(id: generatedCartId, items: [updatedCartItem]);

            var response = CreateCartResponse(generatedCartId, (updatedCartItem.Id, product.Id, product.Price, request.Quantity));

            Cart? createdCart = null;

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _cartRepositoryMock.SetupSequence(repository =>
            repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null).ReturnsAsync(updatedCart);

            _cartRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                .Callback<Cart, CancellationToken>((cart, _) =>
                {
                    createdCart = cart;

                    // Gerçek EF Core SaveChanges sonrasında ID üretirdi.
                    cart.Id = generatedCartId;
                })
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(updatedCart)).Returns(response);


            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.ProductAddedToCart, result.Message);
            Assert.NotNull(result.Data);

            Assert.NotNull(createdCart);
            Assert.Equal(CurrentUserId, createdCart.AppUserId);
            Assert.Equal(generatedCartId, createdCart.Id);

            Assert.Equal(500m, result.Data.TotalPrice);
            Assert.Equal(500m, result.Data.Items.Single().TotalPrice);

            _cartRepositoryMock.Verify(repository => repository.AddAsync(It.Is<Cart>(cart => cart.AppUserId == CurrentUserId), It.IsAny<CancellationToken>()), Times.Once());

            _cartItemRepositoryMock.Verify(repository => repository.AddAsync(It.Is<CartItem>(item =>
            item.CartId == generatedCartId && item.ProductId == product.Id && item.Quantity == request.Quantity), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddItemAsync_Should_IncreaseQuantity_When_ProductAlreadyExistsInCart()
        {
            // Arrange
            var product = CreateProduct(price: 100m, stock: 10);

            var existingItem = CreateCartItem(product: product, quantity: 2);

            var cart = CreateCart(items: [existingItem]);

            var request = new AddToCartRequest(ProductId: product.Id, Quantity: 3);

            var response = CreateCartResponse(cart.Id, (existingItem.Id, product.Id, product.Price, 5));

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(cart)).Returns(response);

            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.ProductAddedToCart, result.Message);

            Assert.Equal(5, existingItem.Quantity);
            Assert.NotNull(result.Data);
            Assert.Equal(500m, result.Data.TotalPrice);

            _cartItemRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AddItemAsync_Should_AddNewItem_When_ProductIsNotInExistingCart()
        {
            // Arrange
            var product = CreateProduct(price: 200m, stock: 10);

            var request = new AddToCartRequest(ProductId: product.Id, Quantity: 3);

            var initialCart = CreateCart();

            var updatedItem = CreateCartItem(id: 5, cartId: initialCart.Id, product: product, quantity: request.Quantity);

            var updatedCart = CreateCart(id: initialCart.Id, items: [updatedItem]);

            var response = CreateCartResponse(updatedCart.Id, (updatedItem.Id, product.Id, product.Price, request.Quantity));

            _productRepositoryMock.Setup(repository => repository.GetByIdWithRelationsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _cartRepositoryMock.SetupSequence(repository =>
            repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(initialCart).ReturnsAsync(updatedCart);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(updatedCart)).Returns(response);

            // Act
            var result = await _cartService.AddItemAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.ProductAddedToCart, result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal(600m, result.Data.TotalPrice);

            _cartRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never());

            _cartItemRepositoryMock.Verify(repository => repository.AddAsync(It.Is<CartItem>(item =>
            item.CartId == initialCart.Id && item.ProductId == product.Id && item.Quantity == request.Quantity), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // CLEAR CART
        // =========================================================

        [Fact]
        public async Task ClearCartAsync_Should_ReturnSuccess_When_CartDoesNotExist()
        {
            // Arrange
            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.ClearCartAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartCleared, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ClearCartAsync_Should_ReturnSuccess_When_CartIsEmpty()
        {
            // Arrange
            var cart = CreateCart();

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.ClearCartAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartCleared, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ClearCartAsync_Should_DeleteAllItems_When_CartHasItems()
        {
            // Arrange
            var firstItem = CreateCartItem(id: 1);
            var secondItem = CreateCartItem(id: 2);

            var cart = CreateCart(items: [firstItem, secondItem]);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.ClearCartAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartCleared, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(firstItem), Times.Once());

            _cartItemRepositoryMock.Verify(repository => repository.Delete(secondItem), Times.Once());

            _cartItemRepositoryMock.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Exactly(2));

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // GET CART
        // =========================================================

        [Fact]
        public async Task GetCartAsync_Should_ReturnExistingCartWithCalculatedTotals()
        {
            // Arrange
            var firstProduct = CreateProduct(id: 10, price: 100m);

            var secondProduct = CreateProduct(id: 20, price: 50m);

            var firstItem = CreateCartItem(id: 1, product: firstProduct, quantity: 2);

            var secondItem = CreateCartItem(id: 2, product: secondProduct, quantity: 3);

            var cart = CreateCart(items: [firstItem, secondItem]);


            var response = CreateCartResponse(cart.Id, (firstItem.Id, firstProduct.Id, firstProduct.Price, 2), (secondItem.Id, secondProduct.Id, secondProduct.Price, 3));

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(cart)).Returns(response);

            // Act
            var result = await _cartService.GetCartAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal(350m, result.Data.TotalPrice);
            Assert.Equal(200m, result.Data.Items[0].TotalPrice);
            Assert.Equal(150m, result.Data.Items[1].TotalPrice);

            _cartRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetCartAsync_Should_CreateEmptyCart_When_CartDoesNotExist()
        {
            // Arrange
            const int generatedCartId = 100;

            Cart? createdCart = null;

            var response = new CartResponse
            {
                Id = generatedCartId,
                Items = []
            };

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

            _cartRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                .Callback<Cart, CancellationToken>((cart, _) =>
                {
                    createdCart = cart;
                    cart.Id = generatedCartId;
                })
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(It.IsAny<Cart>())).Returns(response);

            // Act
            var result = await _cartService.GetCartAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartRetrieved, result.Message);

            Assert.NotNull(createdCart);
            Assert.Equal(CurrentUserId, createdCart.AppUserId);

            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0m, result.Data.TotalPrice);

            _cartRepositoryMock.Verify(repository => repository.AddAsync(It.Is<Cart>(cart => cart.AppUserId == CurrentUserId), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // REMOVE ITEM
        // =========================================================

        [Fact]
        public async Task RemoveItemAsync_Should_ReturnNotFound_When_CartDoesNotExist()
        {
            // Arrange
            const int cartItemId = 10;

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.RemoveItemAsync(cartItemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.CartNotFound, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RemoveItemAsync_Should_ReturnNotFound_When_CartItemDoesNotExist()
        {
            // Arrange
            const int cartItemId = 999;

            var cart = CreateCart();

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.RemoveItemAsync(cartItemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.CartItemNotFound, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RemoveItemAsync_Should_RemoveItem_When_CartItemExists()
        {
            // Arrange
            var cartItem = CreateCartItem(id: 10);

            var cart = CreateCart(items: [cartItem]);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.RemoveItemAsync(cartItem.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartItemRemoved, result.Message);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(cartItem), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // UPDATE ITEM QUANTITY
        // =========================================================

        [Fact]
        public async Task UpdateItemQuantityAsync_Should_ReturnNotFound_When_CartDoesNotExist()
        {
            // Arrange
            const int cartItemId = 10;
            var request = new UpdateCartItemRequest(Quantity: 3);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.UpdateItemQuantityAsync(cartItemId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.CartNotFound, result.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateItemQuantityAsync_Should_ReturnNotFound_When_CartItemDoesNotExist()
        {
            // Arrange
            const int cartItemId = 999;

            var request = new UpdateCartItemRequest(Quantity: 3);
            var cart = CreateCart();

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.UpdateItemQuantityAsync(cartItemId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.CartItemNotFound, result.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateItemQuantityAsync_Should_ReturnBadRequest_When_ProductIsNotActive()
        {
            // Arrange
            var product = CreateProduct(isActive: false);

            var cartItem = CreateCartItem(product: product, quantity: 2);

            var cart = CreateCart(items: [cartItem]);

            var request = new UpdateCartItemRequest(Quantity: 3);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.UpdateItemQuantityAsync(cartItem.Id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.ProductNotActive, result.Message);
            Assert.Equal(2, cartItem.Quantity);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<CartResponse>(It.IsAny<Cart>()), Times.Never());
        }

        [Fact]
        public async Task UpdateItemQuantityAsync_Should_ReturnBadRequest_When_QuantityExceedsStock()
        {
            // Arrange
            var product = CreateProduct(stock: 5, isActive: true);

            var cartItem = CreateCartItem(product: product, quantity: 2);

            var cart = CreateCart(items: [cartItem]);

            var request = new UpdateCartItemRequest(Quantity: 6);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _cartService.UpdateItemQuantityAsync(cartItem.Id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CartMessages.InsufficientStock, result.Message);
            Assert.Equal(2, cartItem.Quantity);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

            _mapperMock.Verify(mapper => mapper.Map<CartResponse>(It.IsAny<Cart>()), Times.Never());

        }

        [Fact]
        public async Task UpdateItemQuantityAsync_Should_UpdateQuantityAndTotals_When_RequestIsValid()
        {
            // Arrange
            var product = CreateProduct(price: 125m, stock: 10, isActive: true);

            var cartItem = CreateCartItem(product: product, quantity: 2);

            var cart = CreateCart(items: [cartItem]);


            var request = new UpdateCartItemRequest(Quantity: 4);

            var response = CreateCartResponse(cart.Id, (cartItem.Id, product.Id, product.Price, request.Quantity));

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            _mapperMock.Setup(mapper => mapper.Map<CartResponse>(cart)).Returns(response);

            // Act
            var result = await _cartService.UpdateItemQuantityAsync(cartItem.Id, request);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CartMessages.CartItemUpdated, result.Message);

            Assert.Equal(4, cartItem.Quantity);

            Assert.NotNull(result.Data);
            Assert.Equal(500m, result.Data.TotalPrice);
            Assert.Equal(500m, result.Data.Items.Single().TotalPrice);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<CartResponse>(cart), Times.Once());
        }


        // =========================================================
        // TEST DATA HELPERS
        // =========================================================

        private static Product CreateProduct(int id = 10, decimal price = 100m, int stock = 10, bool isActive = true)
        {
            return new Product
            {
                Id = id,
                Name = $"Product {id}",
                SKU = $"SKU-{id}",
                Price = price,
                Stock = stock,
                IsActive = isActive,
                BrandId = 1,
                CategoryId = 1
            };
        }

        private static Cart CreateCart(int id = 1, IEnumerable<CartItem>? items = null)
        {
            return new Cart
            {
                Id = id,
                AppUserId = CurrentUserId,
                CartItems = items?.ToList() ?? []
            };
        }

        private static CartItem CreateCartItem(int id = 1, int cartId = 1, Product? product = null, int quantity = 2)
        {
            product ??= CreateProduct();

            return new CartItem
            {
                Id = id,
                CartId = cartId,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity
            };
        }

        private static CartResponse CreateCartResponse(int cartId, params (int ItemId, int ProductId, decimal UnitPrice, int Quantity)[] items)
        {
            return new CartResponse
            {
                Id = cartId,
                Items = items
                .Select(item => new CartItemResponse
                {
                    Id = item.ItemId,
                    ProductId = item.ProductId,
                    ProductName = $"Product {item.ProductId}",
                    MainImageUrl = $"product-{item.ProductId}.jpg",
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }
}
