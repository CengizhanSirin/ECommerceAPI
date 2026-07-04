namespace ECommerce.Application.DTOs.Cart.Responses;

public record CartItemResponse(int Id, int ProductId, string ProductName, string? MainImageUrl, decimal UnitPrice, int Quantity, decimal TotalPrice);