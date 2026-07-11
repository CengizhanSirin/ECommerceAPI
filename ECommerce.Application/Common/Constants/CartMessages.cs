namespace ECommerce.Application.Common.Constants;

public static class CartMessages
{
    // Errors
    public const string CartNotFound = "Sepet bulunamadı.";
    public const string CartItemNotFound = "Sepet ürünü bulunamadı.";
    public const string CartIsEmpty = "Sepetiniz boş.";
    public const string CartItemAlreadyExists = "Bu ürün zaten sepette mevcut.";
    public const string CartItemQuantityExceedsStock = "Sepet ürünü miktarı stok miktarını aşıyor.";
    public const string CartItemQuantityMustBePositive = "Sepet ürünü miktarı pozitif olmalıdır.";
    public const string CartItemQuantityMustBeAtLeastOne = "Sepet ürünü miktarı en az 1 olmalıdır.";
    public const string CartItemQuantityMustBeLessThanOrEqualToStock = "Sepet ürünü miktarı stok miktarına eşit veya daha az olmalıdır.";
    public const string CartItemQuantityMustBeLessThanOrEqualToMaxQuantity = "Sepet ürünü miktarı maksimum miktara eşit veya daha az olmalıdır.";
    public const string ProductNotFound = "Ürün bulunamadı.";
    public const string ProductNotActive = "Bu ürün satışa açık değildir.";
    public const string InsufficientStock = "Ürün için yeterli stok bulunmamaktadır.";


    // Success
    public const string CartCreated = "Sepet başarıyla oluşturuldu.";
    public const string CartItemAdded = "Sepete başarıyla eklendi.";
    public const string CartItemUpdated = "Sepet  başarıyla güncellendi.";
    public const string CartItemRemoved = "Sepet  başarıyla kaldırıldı.";
    public const string CartRetrieved = "Sepet başarıyla getirildi.";
    public const string ProductAddedToCart = "Ürün sepete başarıyla eklendi.";
    public const string CartCleared = "Sepet başarıyla temizlendi.";
}