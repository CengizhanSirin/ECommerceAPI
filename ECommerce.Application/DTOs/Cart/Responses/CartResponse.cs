namespace ECommerce.Application.DTOs.Cart.Responses;

public record CartResponse(int Id, IReadOnlyList<CartItemResponse> Items, decimal TotalPrice);