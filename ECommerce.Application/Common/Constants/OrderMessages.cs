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


    // Success
    public const string OrderCreated = "Sipariş başarıyla oluşturuldu.";
    public const string OrderRetrieved = "Sipariş başarıyla getirildi.";
    public const string OrdersRetrieved = "Siparişler başarıyla getirildi.";
    public const string OrderCancelled = "Sipariş başarıyla iptal edildi.";
    public const string OrderShipped = "Sipariş başarıyla gönderildi.";
    public const string OrderDelivered = "Sipariş başarıyla teslim edildi.";
    public const string OrderReturned = "Sipariş başarıyla iade edildi.";
    public const string OrderCompleted = "Sipariş başarıyla tamamlandı.";

}
