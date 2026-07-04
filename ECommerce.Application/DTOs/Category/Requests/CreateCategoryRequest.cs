namespace ECommerce.Application.DTOs.Category.Requests;

public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl, int DisplayOrder);