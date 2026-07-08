namespace ECommerce.Application.DTOs.Product.Responses;

public sealed class ProductImageResponse
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsMain { get; set; }

    public int DisplayOrder { get; set; }
}