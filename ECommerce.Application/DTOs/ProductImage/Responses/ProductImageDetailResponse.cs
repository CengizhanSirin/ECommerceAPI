namespace ECommerce.Application.DTOs.ProductImage.Responses;

public record ProductImageDetailResponse(int Id, string ImageUrl, bool IsMain, int DisplayOrder, int ProductId, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);