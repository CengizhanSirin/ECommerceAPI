using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.DTOs.Cart.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Authorize]
    public sealed class CartsController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }


        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var result = await _cartService.GetCartAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
        {
            var result = await _cartService.AddItemAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
        {
            var result = await _cartService.UpdateItemQuantityAsync(cartItemId, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken cancellationToken)
        {
            var result = await _cartService.RemoveItemAsync(cartItemId, cancellationToken);
            return HandleNoContent(result);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
        {
            var result = await _cartService.ClearCartAsync(cancellationToken);
            return HandleNoContent(result);
        }
    }
}
