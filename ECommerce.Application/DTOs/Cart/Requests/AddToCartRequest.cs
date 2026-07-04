namespace ECommerce.Application.DTOs.Cart.Requests;

public record AddToCartRequest(int ProductId, int Quantity);