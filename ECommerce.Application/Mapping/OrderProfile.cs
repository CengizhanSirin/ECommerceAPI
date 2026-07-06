using AutoMapper;
using ECommerce.Application.DTOs.Order.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderResponse>();

            CreateMap<Order, OrderListResponse>();

            CreateMap<Order, OrderDetailResponse>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.Ignore());
        }
    }
}
