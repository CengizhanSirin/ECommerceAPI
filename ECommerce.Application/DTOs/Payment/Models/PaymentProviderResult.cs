using ECommerce.Application.Common.Constants;

namespace ECommerce.Application.DTOs.Payment.Models
{
    public sealed class PaymentProviderResult
    {
        public bool IsSuccessful { get; init; }

        public string? TransactionId { get; init; }

        public string Message { get; init; } = null!;

        public static PaymentProviderResult Success(string transactionId)
            => new()
            {
                IsSuccessful = true,
                TransactionId = transactionId,
                Message = PaymentMessages.PaymentSuccessful
            };

        public static PaymentProviderResult Failure(string message)
            => new()
            {
                IsSuccessful = false,
                Message = message
            };
    }
}
