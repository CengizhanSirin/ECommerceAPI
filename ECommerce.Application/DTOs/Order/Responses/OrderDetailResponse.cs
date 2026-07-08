using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Responses;

public sealed class OrderDetailResponse
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public AddressDetailResponse Address { get; set; } = null!;
    public IReadOnlyList<OrderItemResponse> Items { get; set; } = [];
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}