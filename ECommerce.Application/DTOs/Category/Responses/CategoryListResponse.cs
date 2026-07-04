namespace ECommerce.Application.DTOs.Category.Responses;

public record CategoryListResponse(int Id,string Name,string? ImageUrl,bool IsActive,int DisplayOrder);
