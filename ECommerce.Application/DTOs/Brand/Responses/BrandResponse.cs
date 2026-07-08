namespace ECommerce.Application.DTOs.Brand.Responses;

public sealed class BrandResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}