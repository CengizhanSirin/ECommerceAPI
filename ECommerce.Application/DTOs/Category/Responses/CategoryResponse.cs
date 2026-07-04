namespace ECommerce.Application.DTOs.Category.Responses;

public record CategoryResponse(int Id, string Name, string? Description, string? ImageUrl, bool IsActive, int DisplayOrder);