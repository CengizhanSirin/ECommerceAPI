namespace ECommerce.Application.DTOs.Payment.Responses
{
    public sealed class PaymentResponse
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; } = null!;

        public string TransactionId { get; set; } = null!;
    }
}
