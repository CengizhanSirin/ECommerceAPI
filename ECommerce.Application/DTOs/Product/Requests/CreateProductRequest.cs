namespace ECommerce.Application.DTOs.Product.Requests;
public record CreateProductRequest(string Name, string? Description,string SKU, decimal Price, int Stock,int BrandId,int CategoryId);