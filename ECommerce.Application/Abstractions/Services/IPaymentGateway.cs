using ECommerce.Application.DTOs.Payment.Models;
using ECommerce.Application.DTOs.Payment.Requests;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IPaymentGateway
    {
        Task<PaymentProviderResult> ProcessAsync(decimal amount,ProcessPaymentRequest request,CancellationToken cancellationToken = default);
    }
}
