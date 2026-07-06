using AutoMapper;
using ECommerce.Application.DTOs.Brand.Requests;
using ECommerce.Application.DTOs.Brand.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class BrandProfile:Profile
    {
        public BrandProfile()
        {
            CreateMap<Brand, BrandResponse>();
            CreateMap<Brand, BrandDetailResponse>();
            CreateMap<Brand, BrandListResponse>();
            CreateMap<CreateBrandRequest, Brand>();
            CreateMap<UpdateBrandRequest, Brand>();
        }
    }
}
