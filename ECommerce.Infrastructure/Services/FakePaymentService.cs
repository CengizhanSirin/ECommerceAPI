using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Payment.Requests;
using ECommerce.Application.DTOs.Payment.Responses;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Services
{
    public sealed class FakePaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public FakePaymentService(IOrderRepository orderRepository, IPaymentGateway paymentGateway, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultT<PaymentResponse>> ProcessPaymentAsync(int orderId, ProcessPaymentRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var order = await _orderRepository.GetTrackedByIdAsync(orderId, userId, cancellationToken);


            if (order is null)
                return ResultT<PaymentResponse>.NotFound(PaymentMessages.OrderNotFound);

            if (order.OrderStatus == OrderStatus.Cancelled)
                return ResultT<PaymentResponse>.BadRequest(PaymentMessages.OrderCancelled);

            if (order.PaymentStatus == PaymentStatus.Paid)
                return ResultT<PaymentResponse>.BadRequest(PaymentMessages.OrderAlreadyPaid);

            if (order.PaymentStatus == PaymentStatus.Refunded)
                return ResultT<PaymentResponse>.BadRequest(PaymentMessages.PaymentAlreadyRefunded);


            var providerResult = await _paymentGateway.ProcessAsync(order.TotalPrice, request, cancellationToken);

            if (!providerResult.IsSuccessful)
            {
                order.PaymentStatus = PaymentStatus.Failed;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ResultT<PaymentResponse>.BadRequest(providerResult.Message);

            }

            order.PaymentStatus = PaymentStatus.Paid;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new PaymentResponse
            {
                IsSuccessful = true,
                TransactionId = providerResult.TransactionId!,
                Message = providerResult.Message
            };

            return ResultT<PaymentResponse>.Success(response, PaymentMessages.PaymentSuccessful);
        }
    }
}

