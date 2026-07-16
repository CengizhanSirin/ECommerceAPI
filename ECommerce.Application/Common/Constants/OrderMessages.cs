namespace ECommerce.Application.Common.Constants;

public static class OrderMessages
{
    // Errors
    public const string OrderNotFound = "Sipariş bulunamadı.";
    public const string OrderCannotBeCancelled = "Bu sipariş iptal edilemez.";
    public const string CartIsEmpty = "Sepet boş olduğu için sipariş oluşturulamaz.";
    public const string AddressNotFound = "Adres bulunamadı veya kullanıcıya ait değil.";
    public const string ProductNotFound = "Sepetteki ürünlerden biri bulunamadı.";
    public const string ProductNotActive = "Sepetteki ürünlerden biri satışa kapalıdır.";
    public const string InsufficientStock = "Sepetteki ürünlerden biri için yeterli stok bulunmamaktadır.";
    public const string InvalidOrderStatusTransition = "Sipariş durumu bu aşamaya geçirilemez.";


    // Success
    public const string OrderCreated = "Sipariş başarıyla oluşturuldu.";
    public const string OrderRetrieved = "Sipariş başarıyla getirildi.";
    public const string OrdersRetrieved = "Siparişler başarıyla getirildi.";
    public const string OrderCancelled = "Sipariş başarıyla iptal edildi.";
    public const string OrderStatusUpdated = "Sipariş başarıyla güncellendi.";
}
