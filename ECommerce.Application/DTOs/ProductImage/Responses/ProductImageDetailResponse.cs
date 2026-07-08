namespace ECommerce.Application.DTOs.ProductImage.Responses;

public sealed class ProductImageDetailResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}