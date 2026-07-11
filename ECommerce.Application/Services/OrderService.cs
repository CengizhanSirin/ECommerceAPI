using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Order.Requests;
using ECommerce.Application.DTOs.Order.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services
{
    public sealed class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, ICartItemRepository cartItemRepository, IAddressRepository addressRepository,
            IProductRepository productRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var order = await _orderRepository.GetTrackedByIdWithDetailsAsync(id, userId, cancellationToken);

            if (order is null)
                return Result.NotFound(OrderMessages.OrderNotFound);


            if (order.OrderStatus is not OrderStatus.Pending and not OrderStatus.Preparing)
                return Result.BadRequest(OrderMessages.OrderCannotBeCancelled);


            var products = new List<(Product Product, int Quantity)>();

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId, cancellationToken);

                if (product is null)
                {
                    return Result.NotFound(OrderMessages.ProductNotFound);
                }

                products.Add((product, orderItem.Quantity));
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var item in products)
                {
                    item.Product.Stock += item.Quantity;
                }

                order.OrderStatus = OrderStatus.Cancelled;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success(OrderMessages.OrderCancelled);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                throw;
            }
        }

        public async Task<ResultT<OrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var address = await _addressRepository.FirstOrDefaultAsync(x => x.Id == request.AddressId && x.AppUserId == userId, cancellationToken);

            if (address is null)
            {
                return ResultT<OrderResponse>.NotFound(OrderMessages.AddressNotFound);
            }

            var cart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart is null || cart.CartItems.Count == 0)
            {
                return ResultT<OrderResponse>.BadRequest(OrderMessages.CartIsEmpty);
            }

            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;

                if (product is null)
                {
                    return ResultT<OrderResponse>.NotFound(OrderMessages.ProductNotFound);
                }

                if (!product.IsActive)
                {
                    return ResultT<OrderResponse>.BadRequest(OrderMessages.ProductNotActive);
                }

                if (cartItem.Quantity > product.Stock)
                {
                    return ResultT<OrderResponse>.BadRequest(OrderMessages.InsufficientStock);
                }
            }

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                AppUserId = userId,
                AddressId = address.Id,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                TotalPrice = cart.CartItems.Sum(x => x.Product.Price * x.Quantity),
                OrderItems = cart.CartItems.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    UnitPrice = x.Product.Price,
                    Quantity = x.Quantity
                }).ToList()
            };

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await _orderRepository.AddAsync(order, cancellationToken);

                foreach (var cartItem in cart.CartItems)
                {
                    cartItem.Product.Stock -= cartItem.Quantity;

                    _cartItemRepository.Delete(cartItem);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var response = _mapper.Map<OrderResponse>(order);

                return ResultT<OrderResponse>.Success(response, OrderMessages.OrderCreated);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                throw;
            }
        }

        public async Task<ResultT<IReadOnlyList<OrderListResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var orders = await _orderRepository.GetAllByUserIdAsync(userId, cancellationToken);

            var response = _mapper.Map<IReadOnlyList<OrderListResponse>>(orders);

            return ResultT<IReadOnlyList<OrderListResponse>>.Success(response, OrderMessages.OrdersRetrieved);
        }

        public async Task<ResultT<OrderDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var order = await _orderRepository.GetByIdWithDetailsAsync(id, userId, cancellationToken);

            if (order is null)
                return ResultT<OrderDetailResponse>.NotFound(OrderMessages.OrderNotFound);

            var response = MapOrderDetailResponse(order);

            return ResultT<OrderDetailResponse>.Success(response, OrderMessages.OrderRetrieved);
        }

        public async Task<Result> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
                return Result.NotFound(OrderMessages.OrderNotFound);


            if (!IsValidStatusTransition(order.OrderStatus, request.OrderStatus))
                return Result.BadRequest(OrderMessages.InvalidOrderStatusTransition);

            order.OrderStatus = request.OrderStatus;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(OrderMessages.OrderStatusUpdated);
        }


        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }

        private OrderDetailResponse MapOrderDetailResponse(Order order)
        {
            var response = _mapper.Map<OrderDetailResponse>(order);

            foreach (var item in response.Items)
            {
                item.TotalPrice = item.UnitPrice * item.Quantity;
            }

            return response;
        }

        private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus is OrderStatus.Preparing or OrderStatus.Cancelled,

                OrderStatus.Preparing => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled,

                OrderStatus.Shipped => newStatus is OrderStatus.Delivered,

                OrderStatus.Delivered => false,

                OrderStatus.Cancelled => false,

                _ => false
            };
        }
    }
}
