using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Application.DTOs.Order.Requests;
using ECommerce.Application.DTOs.Order.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Orders
{
    public sealed class OrderServiceTests
    {
        private const int CurrentUserId = 1881;

        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
        private readonly Mock<IAddressRepository> _addressRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _cartRepositoryMock = new Mock<ICartRepository>();
            _cartItemRepositoryMock = new Mock<ICartItemRepository>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _currentUserServiceMock.SetupGet(service => service.UserId).Returns(CurrentUserId);

            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _cartRepositoryMock.Object,
                _cartItemRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _productRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object);
        }

        // =========================================================
        // CANCEL
        // =========================================================

        [Fact]
        public async Task CancelAsync_Should_ReturnNotFound_When_OrderDoesNotExist()
        {
            // Arrange
            const int orderId = 999;

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdWithDetailsAsync(orderId, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.CancelAsync(orderId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderNotFound, result.Message);

            _productRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public async Task CancelAsync_Should_ReturnBadRequest_When_OrderStatusCannotBeCancelled(OrderStatus orderStatus)
        {
            // Arrange
            var order = CreateOrder(orderStatus: orderStatus);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdWithDetailsAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.CancelAsync(order.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderCannotBeCancelled, result.Message);

            Assert.Equal(orderStatus, order.OrderStatus);

            _productRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CancelAsync_Should_ReturnNotFound_When_OrderItemProductDoesNotExist()
        {
            // Arrange
            var orderItem = CreateOrderItem(productId: 10, quantity: 2);

            var order = CreateOrder(orderStatus: OrderStatus.Pending, items: [orderItem]);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdWithDetailsAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(orderItem.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _orderService.CancelAsync(order.Id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.ProductNotFound, result.Message);

            Assert.Equal(OrderStatus.Pending, order.OrderStatus);


            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Preparing)]
        public async Task CancelAsync_Should_RestoreStocksAndCancelOrder_When_StatusAllowsCancellation(OrderStatus orderStatus)
        {
            // Arrange
            var firstProduct = CreateProduct(id: 10, stock: 5);

            var secondProduct = CreateProduct(id: 20, stock: 8);

            var firstOrderItem = CreateOrderItem(productId: firstProduct.Id, quantity: 2);

            var secondOrderItem = CreateOrderItem(productId: secondProduct.Id, quantity: 3);

            var order = CreateOrder(orderStatus: orderStatus, items: [firstOrderItem, secondOrderItem]);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdWithDetailsAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(firstProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(firstProduct);

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(secondProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(secondProduct);


            // Act
            var result = await _orderService.CancelAsync(order.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderCancelled, result.Message);

            Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);

            Assert.Equal(7, firstProduct.Stock);
            Assert.Equal(11, secondProduct.Stock);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CancelAsync_Should_RollbackAndRethrow_When_SaveChangesFails()
        {
            // Arrange
            var product = CreateProduct(id: 10, stock: 5);

            var orderItem = CreateOrderItem(productId: product.Id, quantity: 2);

            var order = CreateOrder(orderStatus: OrderStatus.Pending, items: [orderItem]);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdWithDetailsAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _productRepositoryMock.Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Database error"));

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CancelAsync(order.Id));

            // Assert
            Assert.Equal("Database error", exception.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // CREATE
        // =========================================================

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_AddressDoesNotExist()
        {
            // Arrange
            var request = new CreateOrderRequest(AddressId: 999);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Address?)null);

            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.AddressNotFound, result.Message);
            Assert.Null(result.Data);

            _cartRepositoryMock.Verify(repository => repository.GetWithItemsByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnBadRequest_When_CartDoesNotExist()
        {
            // Arrange
            var address = CreateAddress();
            var request = new CreateOrderRequest(address.Id);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.CartIsEmpty, result.Message);
            Assert.Null(result.Data);

            _orderRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnBadRequest_When_CartIsEmpty()
        {
            // Arrange
            var address = CreateAddress();
            var cart = CreateCart();

            var request = new CreateOrderRequest(address.Id);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);


            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.CartIsEmpty, result.Message);
            Assert.Null(result.Data);

            _orderRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnNotFound_When_CartItemProductDoesNotExist()
        {
            // Arrange
            var address = CreateAddress();

            var cartItem = new CartItem
            {
                Id = 1,
                CartId = 1,
                ProductId = 10,
                Product = null!,
                Quantity = 2
            };

            var cart = CreateCart(items: [cartItem]);
            var request = new CreateOrderRequest(address.Id);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.ProductNotFound, result.Message);
            Assert.Null(result.Data);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnBadRequest_When_ProductIsNotActive()
        {
            // Arrange
            var address = CreateAddress();

            var product = CreateProduct(isActive: false);
            var cartItem = CreateCartItem(product: product);
            var cart = CreateCart(items: [cartItem]);

            var request = new CreateOrderRequest(address.Id);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.ProductNotActive, result.Message);
            Assert.Null(result.Data);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnBadRequest_When_StockIsInsufficient()
        {
            // Arrange
            var address = CreateAddress();

            var product = CreateProduct(stock: 2);

            var cartItem = CreateCartItem(product: product, quantity: 3);


            var cart = CreateCart(items: [cartItem]);
            var request = new CreateOrderRequest(address.Id);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.InsufficientStock, result.Message);
            Assert.Null(result.Data);

            Assert.Equal(2, product.Stock);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_CreateOrderAndDecreaseStocks_When_RequestIsValid()
        {
            // Arrange
            var address = CreateAddress();

            var firstProduct = CreateProduct(id: 10, name: "Telefon", price: 100m, stock: 10);

            var secondProduct = CreateProduct(id: 20, name: "Kulaklık", price: 50m, stock: 10);

            var firstCartItem = CreateCartItem(id: 1, product: firstProduct, quantity: 2);

            var secondCartItem = CreateCartItem(id: 2, product: secondProduct, quantity: 3);

            var cart = CreateCart(items: [firstCartItem, secondCartItem]);


            var request = new CreateOrderRequest(address.Id);

            var response = new OrderResponse
            {
                Id = 100,
                OrderNumber = "ORDER-RESPONSE",
                TotalPrice = 350m,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending
            };

            Order? capturedOrder = null;

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            _orderRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Callback<Order, CancellationToken>((order, _) =>
                {
                    capturedOrder = order;
                    order.Id = 100;
                })
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<OrderResponse>(It.IsAny<Order>())).Returns(response);


            // Act
            var result = await _orderService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderCreated, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);

            Assert.NotNull(capturedOrder);

            Assert.Equal(CurrentUserId, capturedOrder.AppUserId);
            Assert.Equal(address.Id, capturedOrder.AddressId);
            Assert.Equal(OrderStatus.Pending, capturedOrder.OrderStatus);
            Assert.Equal(PaymentStatus.Pending, capturedOrder.PaymentStatus);
            Assert.Equal(350m, capturedOrder.TotalPrice);
            Assert.StartsWith("ORD-", capturedOrder.OrderNumber);

            Assert.Equal(2, capturedOrder.OrderItems.Count);

            Assert.Contains(capturedOrder.OrderItems, item =>
                    item.ProductId == firstProduct.Id &&
                    item.ProductName == firstProduct.Name &&
                    item.UnitPrice == firstProduct.Price &&
                    item.Quantity == firstCartItem.Quantity);

            Assert.Contains(capturedOrder.OrderItems, item =>
                    item.ProductId == secondProduct.Id &&
                    item.ProductName == secondProduct.Name &&
                    item.UnitPrice == secondProduct.Price &&
                    item.Quantity == secondCartItem.Quantity);

            Assert.Equal(8, firstProduct.Stock);
            Assert.Equal(7, secondProduct.Stock);

            _cartItemRepositoryMock.Verify(repository => repository.Delete(firstCartItem), Times.Once());

            _cartItemRepositoryMock.Verify(repository => repository.Delete(secondCartItem), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_RollbackAndRethrow_When_SaveChangesFails()
        {
            // Arrange
            var address = CreateAddress();
            var product = CreateProduct(stock: 10);

            var cartItem = CreateCartItem(product: product, quantity: 2);

            var cart = CreateCart(items: [cartItem]);
            var request = new CreateOrderRequest(address.Id);


            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _cartRepositoryMock.Setup(repository => repository.GetWithItemsByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            _orderRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Database error"));

            _unitOfWorkMock.Setup(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);


            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateAsync(request));

            // Assert
            Assert.Equal("Database error", exception.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<OrderResponse>(It.IsAny<Order>()), Times.Never());
        }

        // =========================================================
        // GET ALL
        // =========================================================


        [Fact]
        public async Task GetAllAsync_Should_ReturnUserOrders()
        {
            // Arrange
            var orders = new List<Order> { CreateOrder(id: 1), CreateOrder(id: 2) };

            IReadOnlyList<OrderListResponse> responses = new List<OrderListResponse>
            {
                new()
                {
                    Id = 1,
                    OrderNumber = orders[0].OrderNumber,
                    TotalPrice = orders[0].TotalPrice,
                    OrderStatus = orders[0].OrderStatus,
                    PaymentStatus = orders[0].PaymentStatus
                },
                new()
                {
                    Id = 2,
                    OrderNumber = orders[1].OrderNumber,
                    TotalPrice = orders[1].TotalPrice,
                    OrderStatus = orders[1].OrderStatus,
                    PaymentStatus = orders[1].PaymentStatus
                }
            };

            _orderRepositoryMock.Setup(repository => repository.GetAllByUserIdAsync(CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

            _mapperMock.Setup(mapper => mapper.Map<IReadOnlyList<OrderListResponse>>(orders)).Returns(responses);

            // Act
            var result = await _orderService.GetAllAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(OrderMessages.OrdersRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(responses, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<IReadOnlyList<OrderListResponse>>(orders), Times.Once());
        }

        // =========================================================
        // GET BY ID
        // =========================================================


        [Fact]
        public async Task GetByIdAsync_Should_ReturnNotFound_When_OrderDoesNotExist()
        {
            // Arrange
            const int orderId = 999;

            _orderRepositoryMock.Setup(repository => repository.GetByIdWithDetailsAsync(orderId, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.GetByIdAsync(orderId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderNotFound, result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<OrderDetailResponse>(It.IsAny<Order>()), Times.Never());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnOrderWithCalculatedItemTotals_When_OrderExists()
        {
            // Arrange
            var firstOrderItem = CreateOrderItem(productId: 10, quantity: 2, unitPrice: 100m);

            var secondOrderItem = CreateOrderItem(productId: 20, quantity: 3, unitPrice: 50m);

            var order = CreateOrder(totalPrice: 350m, items: [firstOrderItem, secondOrderItem]);

            var response = new OrderDetailResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalPrice = order.TotalPrice,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                Address = CreateAddressDetailResponse(order.Address),
                Items =
                [
                    new OrderItemResponse
                {
                    ProductId = firstOrderItem.ProductId,
                    ProductName = firstOrderItem.ProductName,
                    UnitPrice = firstOrderItem.UnitPrice,
                    Quantity = firstOrderItem.Quantity
                },
                    new  OrderItemResponse
                {
                    ProductId = secondOrderItem.ProductId,
                    ProductName = secondOrderItem.ProductName,
                    UnitPrice = secondOrderItem.UnitPrice,
                    Quantity = secondOrderItem.Quantity
                }
                ]
            };

            _orderRepositoryMock.Setup(repository => repository.GetByIdWithDetailsAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _mapperMock.Setup(mapper => mapper.Map<OrderDetailResponse>(order)).Returns(response);

            // Act
            var result = await _orderService.GetByIdAsync(order.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);

            Assert.Equal(200m, result.Data.Items[0].TotalPrice);
            Assert.Equal(150m, result.Data.Items[1].TotalPrice);

            _mapperMock.Verify(mapper => mapper.Map<OrderDetailResponse>(order), Times.Once());
        }

        // =========================================================
        // UPDATE STATUS
        // =========================================================


        [Fact]
        public async Task UpdateStatusAsync_Should_ReturnNotFound_When_OrderDoesNotExist()
        {
            // Arrange
            const int orderId = 999;

            var request = new UpdateOrderStatusRequest(OrderStatus.Preparing);

            _orderRepositoryMock.Setup(repository => repository.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.UpdateStatusAsync(orderId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderNotFound, result.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Preparing, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Preparing)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Preparing)]
        public async Task UpdateStatusAsync_Should_ReturnBadRequest_When_StatusTransitionIsInvalid(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Arrange
            var order = CreateOrder(orderStatus: currentStatus);

            var request = new UpdateOrderStatusRequest(newStatus);

            _orderRepositoryMock.Setup(repository => repository.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.UpdateStatusAsync(order.Id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(OrderMessages.InvalidOrderStatusTransition, result.Message);

            Assert.Equal(currentStatus, order.OrderStatus);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Preparing)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Preparing, OrderStatus.Shipped)]
        [InlineData(OrderStatus.Preparing, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
        public async Task UpdateStatusAsync_Should_UpdateStatus_When_TransitionIsValid(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Arrange
            var order = CreateOrder(orderStatus: currentStatus);

            var request = new UpdateOrderStatusRequest(newStatus);

            _orderRepositoryMock.Setup(repository => repository.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.UpdateStatusAsync(order.Id, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(OrderMessages.OrderStatusUpdated, result.Message);


            Assert.Equal(newStatus, order.OrderStatus);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }



        // =========================================================
        // TEST DATA HELPERS
        // =========================================================


        private static Address CreateAddress(int id = 1)
        {
            return new Address
            {
                Id = id,
                AppUserId = CurrentUserId,
                Title = "Ev",
                FirstName = "Cengizhan",
                LastName = "Şirin",
                Phone = "05555555555",
                City = "İstanbul",
                District = "Kadıköy",
                Neighborhood = "Caferağa",
                Street = "Moda Caddesi No: 1",
                PostalCode = "34710",
                IsDefault = true
            };
        }

        private static AddressDetailResponse CreateAddressDetailResponse(Address address)
        {
            return new AddressDetailResponse
            {
                Id = address.Id,
                Title = address.Title,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Phone = address.Phone,
                City = address.City,
                District = address.District,
                Neighborhood = address.Neighborhood,
                Street = address.Street,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault,
                CreatedDate = address.CreatedDate,
                UpdatedDate = address.UpdatedDate
            };
        }

        private static Product CreateProduct(int id = 10, string? name = null, decimal price = 100m, int stock = 10, bool isActive = true)
        {
            return new Product
            {
                Id = id,
                Name = name ?? $"Product {id}",
                SKU = $"SKU-{id}",
                Price = price,
                Stock = stock,
                IsActive = isActive,
                BrandId = 1,
                CategoryId = 1
            };
        }

        private static CartItem CreateCartItem(int id = 1, Product? product = null, int quantity = 2)
        {
            product ??= CreateProduct();

            return new CartItem
            {
                Id = id,
                CartId = 1,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity
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

        private static OrderItem CreateOrderItem(int productId = 10, int quantity = 2, decimal unitPrice = 100m)
        {
            return new OrderItem
            {
                ProductId = productId,
                ProductName = $"Product {productId}",
                UnitPrice = unitPrice,
                Quantity = quantity
            };
        }

        private static Order CreateOrder(int id = 1, decimal totalPrice = 350m,
            OrderStatus orderStatus = OrderStatus.Pending, PaymentStatus paymentStatus = PaymentStatus.Pending, IEnumerable<OrderItem>? items = null)
        {
            var address = CreateAddress();

            return new Order
            {
                Id = id,
                OrderNumber = $"ORDER-{id}",
                TotalPrice = totalPrice,
                OrderStatus = orderStatus,
                PaymentStatus = paymentStatus,
                AppUserId = CurrentUserId,
                AddressId = address.Id,
                Address = address,
                OrderItems = items?.ToList() ?? []
            };
        }
    }
}
