using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Cart.Requests;
using ECommerce.Application.DTOs.Cart.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface ICartService
    {
        Task<ResultT<CartResponse>> GetCartAsync(CancellationToken cancellationToken = default);
        Task<ResultT<CartResponse>> AddItemAsync(AddToCartRequest request, CancellationToken cancellationToken = default);
        Task<ResultT<CartResponse>> UpdateItemQuantityAsync(int cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
        Task<Result> RemoveItemAsync(int cartItemId, CancellationToken cancellationToken = default);
        Task<Result> ClearCartAsync(CancellationToken cancellationToken = default);
    }
}
