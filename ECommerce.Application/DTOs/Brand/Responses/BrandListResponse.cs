namespace ECommerce.Application.DTOs.Brand.Responses;

public sealed class BrandListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}

