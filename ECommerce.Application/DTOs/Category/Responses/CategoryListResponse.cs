namespace ECommerce.Application.DTOs.Category.Responses;

public sealed class CategoryListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
