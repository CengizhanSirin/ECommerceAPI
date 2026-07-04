namespace ECommerce.Application.DTOs.ProductImage.Responses;

public record ProductImageResponse(int Id,string ImageUrl,bool IsMain,int DisplayOrder,int ProductId);