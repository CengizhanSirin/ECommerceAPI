using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Order.Requests;
using ECommerce.Application.DTOs.Payment.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Authorize]
    public sealed class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrdersController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpPost("{orderId:int}/payment")]
        public async Task<IActionResult> ProcessPayment(int orderId, [FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
        {
            var result = await _paymentService.ProcessPaymentAsync(orderId, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var result = await _orderService.CreateAsync(request, cancellationToken);
            return HandleCreatedResult(result, nameof(GetById), data => new { id = data.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _orderService.GetAllAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _orderService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _orderService.CancelAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _orderService.UpdateStatusAsync(id, request, cancellationToken);
            return HandleResult(result);
        }
    }
}
