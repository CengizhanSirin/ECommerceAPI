namespace ECommerce.Application.Common.Constants;

public static class OrderMessages
{
    // Errors
    public const string OrderNotFound = "Sipariş bulunamadı.";
    public const string OrderCannotBeCancelled = "Bu sipariş iptal edilemez.";
    public const string OrderCannotBeShipped = "Bu sipariş gönderilemez.";
    public const string OrderCannotBeDelivered = "Bu sipariş teslim edilemez.";
    public const string OrderCannotBeReturned = "Bu sipariş iade edilemez.";
    public const string OrderCannotBeRefunded = "Bu sipariş geri ödenemez.";
    public const string OrderCannotBeCompleted = "Bu sipariş tamamlanamaz.";
    public const string OrderCreateFailed = "Sipariş oluşturulurken bir hata oluştu.";
    public const string CartNotFound = "Sepet bulunamadı.";
    public const string CartIsEmpty = "Sepet boş olduğu için sipariş oluşturulamaz.";
    public const string AddressNotFound = "Adres bulunamadı veya kullanıcıya ait değil.";
    public const string ProductNotFound = "Sepetteki ürünlerden biri bulunamadı.";
    public const string ProductNotActive = "Sepetteki ürünlerden biri satışa kapalıdır.";
    public const string InsufficientStock = "Sepetteki ürünlerden biri için yeterli stok bulunmamaktadır.";
    public const string InvalidOrderStatusTransition ="Sipariş durumu bu aşamaya geçirilemez.";


    // Success
    public const string OrderCreated = "Sipariş başarıyla oluşturuldu.";
    public const string OrderRetrieved = "Sipariş başarıyla getirildi.";
    public const string OrdersRetrieved = "Siparişler başarıyla getirildi.";
    public const string OrderCancelled = "Sipariş başarıyla iptal edildi.";
    public const string OrderShipped = "Sipariş başarıyla gönderildi.";
    public const string OrderDelivered = "Sipariş başarıyla teslim edildi.";
    public const string OrderReturned = "Sipariş başarıyla iade edildi.";
    public const string OrderCompleted = "Sipariş başarıyla tamamlandı.";
    public const string OrderStatusUpdated = "Sipariş başarıyla güncellendi.";
}
