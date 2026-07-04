namespace ECommerce.Application.DTOs.Order.Responses;

public record OrderItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal TotalPrice);