using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Payment.Models;
using ECommerce.Application.DTOs.Payment.Requests;

namespace ECommerce.Infrastructure.Payments
{
    public sealed class FakePaymentGateway : IPaymentGateway
    {
        public Task<PaymentProviderResult> ProcessAsync(decimal amount, ProcessPaymentRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cardNumber = request.CardNumber.Replace(" ", string.Empty);

            var result = cardNumber switch
            {
                "4111111111111111" => PaymentProviderResult.Success($"FAKE-{Guid.NewGuid():N}"),

                "4000000000009995" => PaymentProviderResult.Failure(PaymentMessages.InsufficientFunds),

                "4000000000000002" => PaymentProviderResult.Failure(PaymentMessages.InvalidCard),

                _ => PaymentProviderResult.Failure(PaymentMessages.PaymentFailed)
            };

            return Task.FromResult(result);
        }
    }
}
