namespace ECommerce.Application.Common.Constants
{
    public static class PaymentMessages
    {
        // Errors
        public const string PaymentFailed = "Ödeme başarısız oldu.";
        public const string InsufficientFunds = "Kartta yeterli bakiye bulunmamaktadır.";
        public const string InvalidCard = "Kart bilgileri geçersiz.";
        public const string OrderNotFound = "Sipariş bulunamadı.";
        public const string OrderAlreadyPaid = "Bu sipariş zaten ödenmiş.";
        public const string OrderCancelled = "İptal edilen sipariş ödenemez.";
        public const string PaymentAlreadyRefunded = "İade edilmiş bir sipariş tekrar ödenemez.";


        // Success
        public const string PaymentSuccessful = "Ödeme başarıyla tamamlandı.";
    }
}
