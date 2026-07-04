namespace ECommerce.Application.DTOs.Product.Responses;

public record ProductListResponse(int Id, string Name, decimal Price, int Stock, bool IsActive, string? MainImageUrl, string CategoryName, string BrandName);