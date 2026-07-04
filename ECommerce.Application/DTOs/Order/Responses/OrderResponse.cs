using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Responses;

public record OrderResponse(int Id, string OrderNumber, decimal TotalPrice, OrderStatus OrderStatus, PaymentStatus PaymentStatus, DateTimeOffset CreatedDate);