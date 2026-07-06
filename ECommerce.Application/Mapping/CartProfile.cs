using AutoMapper;
using ECommerce.Application.DTOs.Cart.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<Cart, CartResponse>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());

            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.Product.ProductImages.Where(x => x.IsMain).Select(x => x.ImageUrl).FirstOrDefault()))
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
        }
    }
}
