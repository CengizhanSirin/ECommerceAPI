namespace ECommerce.Application.DTOs.ProductImage.Requests;

public record UpdateProductImageRequest(string ImageUrl, bool IsMain, int DisplayOrder);