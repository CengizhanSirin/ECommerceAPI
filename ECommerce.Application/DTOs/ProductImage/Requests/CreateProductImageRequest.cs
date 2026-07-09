namespace ECommerce.Application.DTOs.ProductImage.Requests;

public record CreateProductImageRequest(string ImageUrl, bool IsMain,int DisplayOrder);