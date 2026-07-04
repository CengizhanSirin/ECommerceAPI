namespace ECommerce.Application.DTOs.Brand.Requests;

public record UpdateBrandRequest(string Name, string? Description, string? LogoUrl, bool IsActive);