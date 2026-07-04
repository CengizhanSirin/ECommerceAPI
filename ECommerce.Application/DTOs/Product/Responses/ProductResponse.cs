namespace ECommerce.Application.DTOs.Product.Responses;

public record ProductResponse(int Id, string Name, string? Description, string SKU, decimal Price, int Stock, bool IsActive, int CategoryId, string CategoryName,
  int BrandId, string BrandName);