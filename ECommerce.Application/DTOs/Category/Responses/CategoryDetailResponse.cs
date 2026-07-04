namespace ECommerce.Application.DTOs.Category.Responses;

public record CategoryDetailResponse(int Id, string Name, string? Description, string? ImageUrl, bool IsActive, int DisplayOrder, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);