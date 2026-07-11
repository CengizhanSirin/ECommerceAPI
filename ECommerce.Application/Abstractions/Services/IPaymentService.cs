using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Payment.Requests;
using ECommerce.Application.DTOs.Payment.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IPaymentService
    {
        Task<ResultT<PaymentResponse>> ProcessPaymentAsync(int orderId, ProcessPaymentRequest request, CancellationToken cancellationToken = default);
    }
}
