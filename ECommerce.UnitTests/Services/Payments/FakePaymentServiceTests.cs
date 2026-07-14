using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Payment.Models;
using ECommerce.Application.DTOs.Payment.Requests;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Services;
using Moq;

namespace ECommerce.UnitTests.Services.Payments
{
    public sealed class FakePaymentServiceTests
    {
        private const int CurrentUserId = 14;

        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPaymentGateway> _paymentGatewayMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly FakePaymentService _paymentService;

        public FakePaymentServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _paymentGatewayMock = new Mock<IPaymentGateway>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _currentUserServiceMock.SetupGet(service => service.UserId).Returns(CurrentUserId);

            _paymentService = new FakePaymentService(_orderRepositoryMock.Object, _paymentGatewayMock.Object, _currentUserServiceMock.Object, _unitOfWorkMock.Object);
        }

        // =========================================================
        // ORDER CONTROLS
        // =========================================================


        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnNotFound_When_OrderDoesNotExist()
        {
            // Arrange
            const int orderId = 999;
            var request = CreateValidPaymentRequest();

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(orderId, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(orderId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentMessages.OrderNotFound, result.Message);
            Assert.Null(result.Data);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(It.IsAny<decimal>(), It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnBadRequest_When_OrderIsCancelled()
        {
            // Arrange
            var order = CreateOrder(orderStatus: OrderStatus.Cancelled, paymentStatus: PaymentStatus.Pending);

            var request = CreateValidPaymentRequest();

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentMessages.OrderCancelled, result.Message);
            Assert.Null(result.Data);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(It.IsAny<decimal>(), It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnBadRequest_When_OrderIsAlreadyPaid()
        {
            // Arrange
            var order = CreateOrder(orderStatus: OrderStatus.Pending, paymentStatus: PaymentStatus.Paid);

            var request = CreateValidPaymentRequest();

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentMessages.OrderAlreadyPaid, result.Message);
            Assert.Null(result.Data);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(It.IsAny<decimal>(), It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnBadRequest_When_PaymentIsRefunded()
        {
            // Arrange
            var order = CreateOrder(orderStatus: OrderStatus.Pending, paymentStatus: PaymentStatus.Refunded);

            var request = CreateValidPaymentRequest();

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);


            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentMessages.PaymentAlreadyRefunded, result.Message);

            Assert.Null(result.Data);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(It.IsAny<decimal>(), It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }


        // =========================================================
        // GATEWAY RESULTS
        // =========================================================

        [Fact]
        public async Task ProcessPaymentAsync_Should_SetPaymentStatusToFailed_When_GatewayFails()
        {
            // Arrange
            var order = CreateOrder(paymentStatus: PaymentStatus.Pending);

            var request = CreateValidPaymentRequest();

            var providerResult = PaymentProviderResult.Failure(PaymentMessages.InsufficientFunds);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _paymentGatewayMock.Setup(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>())).ReturnsAsync(providerResult);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);


            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentMessages.InsufficientFunds, result.Message);

            Assert.Null(result.Data);
            Assert.Equal(PaymentStatus.Failed, order.PaymentStatus);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_SetPaymentStatusToPaid_When_GatewaySucceeds()
        {
            // Arrange
            const string transactionId = "FAKE-123456";

            var order = CreateOrder(paymentStatus: PaymentStatus.Pending);

            var request = CreateValidPaymentRequest();

            var providerResult = PaymentProviderResult.Success(transactionId);


            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _paymentGatewayMock.Setup(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>())).ReturnsAsync(providerResult);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentMessages.PaymentSuccessful, result.Message);

            Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);

            Assert.NotNull(result.Data);
            Assert.True(result.Data.IsSuccessful);
            Assert.Equal(transactionId, result.Data.TransactionId);
            Assert.Equal(PaymentMessages.PaymentSuccessful, result.Data.Message);


            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_RetryPayment_When_PreviousPaymentFailed()
        {
            // Arrange
            const string transactionId = "FAKE-RETRY-123";

            var order = CreateOrder(paymentStatus: PaymentStatus.Failed);

            var request = CreateValidPaymentRequest();

            var providerResult = PaymentProviderResult.Success(transactionId);

            _orderRepositoryMock.Setup(repository => repository.GetTrackedByIdAsync(order.Id, CurrentUserId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            _paymentGatewayMock.Setup(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>())).ReturnsAsync(providerResult);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(order.Id, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentMessages.PaymentSuccessful, result.Message);

            Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);

            Assert.NotNull(result.Data);
            Assert.True(result.Data.IsSuccessful);
            Assert.Equal(transactionId, result.Data.TransactionId);

            _paymentGatewayMock.Verify(gateway => gateway.ProcessAsync(order.TotalPrice, request, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // TEST DATA HELPERS
        // =========================================================

        private static ProcessPaymentRequest CreateValidPaymentRequest()
        {
            return new ProcessPaymentRequest(
                CardHolderName: "Cengizhan Şirin",
                CardNumber: "4111111111111111",
                ExpirationMonth: "12",
                ExpirationYear: "2030",
                Cvv: "123");
        }

        private static Order CreateOrder(int id = 1, decimal totalPrice = 1_500m, OrderStatus orderStatus = OrderStatus.Pending, PaymentStatus paymentStatus = PaymentStatus.Pending)
        {
            return new Order
            {
                Id = id,
                OrderNumber = $"ORDER-{id}",
                TotalPrice = totalPrice,
                OrderStatus = orderStatus,
                PaymentStatus = paymentStatus,
                AppUserId = CurrentUserId,
                AddressId = 1
            };
        }
    }
}
