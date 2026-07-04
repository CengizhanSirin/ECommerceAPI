namespace ECommerce.Application.DTOs.Product.Responses;

public record ProductDetailResponse(int Id, string Name, string? Description, string SKU, decimal Price, int Stock, bool IsActive, int CategoryId, string CategoryName,
    int BrandId, string BrandName, IReadOnlyList<string> Images, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);
