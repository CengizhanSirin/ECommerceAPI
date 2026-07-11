using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Cart.Requests;
using ECommerce.Application.DTOs.Cart.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository, ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultT<CartResponse>> AddItemAsync(AddToCartRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var product = await _productRepository.GetByIdWithRelationsAsync(request.ProductId, cancellationToken);

            if (product is null)
                return ResultT<CartResponse>.NotFound(CartMessages.ProductNotFound);

            if (!product.IsActive)
                return ResultT<CartResponse>.BadRequest(CartMessages.ProductNotActive);

            var cart = await GetOrCreateCartAsync(userId, cancellationToken);

            var existingItem = cart.CartItems.FirstOrDefault(x => x.ProductId == request.ProductId);


            var newQuantity = existingItem is null
                ? request.Quantity
                : existingItem.Quantity + request.Quantity;

            if (newQuantity > product.Stock)
                return ResultT<CartResponse>.BadRequest(CartMessages.InsufficientStock);

            if (existingItem is not null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = request.Quantity
                };

                await _cartItemRepository.AddAsync(cartItem, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedCart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);
            return ResultT<CartResponse>.Success(MapCartResponse(updatedCart!), CartMessages.ProductAddedToCart);
        }

        public async Task<Result> ClearCartAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var cart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart is null || cart.CartItems.Count == 0)
                return Result.Success(CartMessages.CartCleared);

            foreach (var cartItem in cart.CartItems)
                _cartItemRepository.Delete(cartItem);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CartMessages.CartCleared);
        }

        public async Task<ResultT<CartResponse>> GetCartAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var cart = await GetOrCreateCartAsync(userId, cancellationToken);

            return ResultT<CartResponse>.Success(MapCartResponse(cart), CartMessages.CartRetrieved);
        }

        public async Task<Result> RemoveItemAsync(int cartItemId, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var cart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart is null)
                return Result.NotFound(CartMessages.CartNotFound);

            var cartItem = cart.CartItems.FirstOrDefault(x => x.Id == cartItemId);

            if (cartItem is null)
                return Result.NotFound(CartMessages.CartItemNotFound);

            _cartItemRepository.Delete(cartItem);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CartMessages.CartItemRemoved);
        }

        public async Task<ResultT<CartResponse>> UpdateItemQuantityAsync(int cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var cart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart is null)
                return ResultT<CartResponse>.NotFound(CartMessages.CartNotFound);

            var cartItem = cart.CartItems.FirstOrDefault(x => x.Id == cartItemId);

            if (cartItem is null)
                return ResultT<CartResponse>.NotFound(CartMessages.CartItemNotFound);

            if (!cartItem.Product.IsActive)
                return ResultT<CartResponse>.BadRequest(CartMessages.ProductNotActive);

            if (request.Quantity > cartItem.Product.Stock)
                return ResultT<CartResponse>.BadRequest(CartMessages.InsufficientStock);

            cartItem.Quantity = request.Quantity;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ResultT<CartResponse>.Success(MapCartResponse(cart), CartMessages.CartItemUpdated);
        }

        private CartResponse MapCartResponse(Cart cart)
        {
            var response = _mapper.Map<CartResponse>(cart);

            foreach (var item in response.Items)
            {
                item.TotalPrice = item.UnitPrice * item.Quantity;
            }

            response.TotalPrice = response.Items.Sum(x => x.TotalPrice);

            return response;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart is not null)
                return cart;

            cart = new Cart
            {
                AppUserId = userId
            };

            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return cart;
        }
    }
}
