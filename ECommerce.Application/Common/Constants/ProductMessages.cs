namespace ECommerce.Application.Common.Constants;

public static class ProductMessages
{
    // Errors
    public const string ProductNotFound = "Ürün bulunamadı.";
    public const string ProductSkuAlreadyExists = "Bu SKU zaten kullanılmaktadır.";
    public const string ProductCategoryNotFound = "Ürüne ait kategori bulunamadı.";
    public const string ProductBrandNotFound = "Ürüne ait marka bulunamadı.";

    // Success
    public const string ProductCreated = "Ürün başarıyla oluşturuldu.";
    public const string ProductUpdated = "Ürün başarıyla güncellendi.";
    public const string ProductDeleted = "Ürün başarıyla silindi.";
    public const string ProductRetrieved = "Ürün başarıyla getirildi.";
    public const string ProductsRetrieved = "Ürünler başarıyla getirildi.";
}
