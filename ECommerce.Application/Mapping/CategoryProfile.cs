using AutoMapper;
using ECommerce.Application.DTOs.Category.Requests;
using ECommerce.Application.DTOs.Category.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResponse>();

            CreateMap<Category, CategoryListResponse>();

            CreateMap<Category, CategoryDetailResponse>();

            CreateMap<CreateCategoryRequest, Category>();

            CreateMap<UpdateCategoryRequest, Category>();
        }
    }
}
