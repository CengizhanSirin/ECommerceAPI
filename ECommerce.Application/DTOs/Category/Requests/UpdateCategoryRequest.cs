namespace ECommerce.Application.DTOs.Category.Requests;

public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl, int DisplayOrder, bool IsActive);
