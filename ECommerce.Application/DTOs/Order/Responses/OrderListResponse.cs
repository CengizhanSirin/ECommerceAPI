using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Responses;

public record OrderListResponse(int Id, string OrderNumber,decimal TotalPrice, OrderStatus OrderStatus, PaymentStatus PaymentStatus, DateTimeOffset CreatedDate);