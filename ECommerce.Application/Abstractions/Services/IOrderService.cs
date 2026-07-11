using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Order.Requests;
using ECommerce.Application.DTOs.Order.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IOrderService
    {
        Task<ResultT<OrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<ResultT<IReadOnlyList<OrderListResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<ResultT<OrderDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);

        Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default);

    }
}
