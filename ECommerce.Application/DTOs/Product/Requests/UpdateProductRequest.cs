namespace ECommerce.Application.DTOs.Product.Requests;
public record UpdateProductRequest(string Name, string? Description, decimal Price, int Stock, bool IsActive, int BrandId, int CategoryId);