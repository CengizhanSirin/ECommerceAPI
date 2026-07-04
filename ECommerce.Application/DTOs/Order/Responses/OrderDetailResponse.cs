using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Responses;

public record OrderDetailResponse(int Id, string OrderNumber, decimal TotalPrice, OrderStatus OrderStatus, PaymentStatus PaymentStatus, AddressDetailResponse Address, IReadOnlyList<OrderItemResponse> Items, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);