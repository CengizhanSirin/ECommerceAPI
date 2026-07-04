namespace ECommerce.Application.DTOs.Brand.Responses;

public record BrandListResponse(int Id, string Name,string? LogoUrl,bool IsActive);

