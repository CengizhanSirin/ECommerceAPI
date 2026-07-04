using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Requests;

public record UpdateOrderStatusRequest(OrderStatus OrderStatus);