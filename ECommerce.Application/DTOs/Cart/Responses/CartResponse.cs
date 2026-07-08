namespace ECommerce.Application.DTOs.Cart.Responses;

public sealed class CartResponse
{
    public int Id { get; set; }
    public IReadOnlyList<CartItemResponse> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}