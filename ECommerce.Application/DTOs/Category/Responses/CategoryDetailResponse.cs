namespace ECommerce.Application.DTOs.Category.Responses;

public sealed class CategoryDetailResponse
{
    public int Id { get; set; }   
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}