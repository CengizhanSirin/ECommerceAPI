using AutoMapper;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.Product.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class ProductProfile:Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductResponse>();

            CreateMap<Product, ProductListResponse>();

            CreateMap<Product, ProductDetailResponse>();

            CreateMap<CreateProductRequest, Product>();

            CreateMap<UpdateProductRequest, Product>();
        }
    }
}
