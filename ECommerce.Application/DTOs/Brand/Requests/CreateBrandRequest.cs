namespace ECommerce.Application.DTOs.Brand.Requests;

public record CreateBrandRequest(string Name, string? Description, string? LogoUrl);