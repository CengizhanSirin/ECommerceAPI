namespace ECommerce.Application.DTOs.Product.Responses;
public record ProductImageResponse(int Id,string ImageUrl,bool IsMain,int DisplayOrder);