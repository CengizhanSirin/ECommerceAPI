using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order.Responses;

public sealed class OrderListResponse
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}