namespace ECommerce.Application.DTOs.Brand.Responses;

public record BrandDetailResponse(int Id, string Name, string? Description, string? LogoUrl, bool IsActive, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);