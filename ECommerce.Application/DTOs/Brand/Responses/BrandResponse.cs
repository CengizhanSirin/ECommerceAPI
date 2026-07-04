namespace ECommerce.Application.DTOs.Brand.Responses;

public record BrandResponse(int Id, string Name, string? Description, string? LogoUrl, bool IsActive);