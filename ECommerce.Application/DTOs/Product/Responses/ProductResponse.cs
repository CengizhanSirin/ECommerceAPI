namespace ECommerce.Application.DTOs.Product.Responses;

public sealed class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
}