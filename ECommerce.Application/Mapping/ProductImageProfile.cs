using AutoMapper;
using ECommerce.Application.DTOs.ProductImage.Requests;
using ECommerce.Application.DTOs.ProductImage.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class ProductImageProfile:Profile
    {
        public ProductImageProfile()
        {
            CreateMap<ProductImage, ProductImageResponse>();

            CreateMap<CreateProductImageRequest, ProductImage>();

            CreateMap<UpdateProductImageRequest, ProductImage>();
        }
    }
}
