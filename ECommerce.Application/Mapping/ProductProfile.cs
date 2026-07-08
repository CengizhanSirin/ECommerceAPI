using AutoMapper;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.Product.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<CreateProductRequest, Product>();

            CreateMap<UpdateProductRequest, Product>();

            CreateMap<Product, ProductDetailResponse>()
                 .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                 .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                 .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImageUrl).ToList()));

            CreateMap<Product, ProductListResponse>()
                 .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                 .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                 .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.ProductImages
                 .Where(x => x.IsMain)
                 .Select(x => x.ImageUrl)
                 .FirstOrDefault()));

            CreateMap<Product, ProductResponse>()
                 .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                 .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));
        }
    }
}
