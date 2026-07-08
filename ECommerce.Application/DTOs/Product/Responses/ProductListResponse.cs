namespace ECommerce.Application.DTOs.Product.Responses;

public sealed class ProductListResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    public string? MainImageUrl { get; set; }

    public string CategoryName { get; set; } = null!;

    public string BrandName { get; set; } = null!;
}