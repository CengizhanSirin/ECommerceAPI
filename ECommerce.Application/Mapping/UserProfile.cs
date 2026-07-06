using AutoMapper;
using ECommerce.Application.DTOs.User.Requests;
using ECommerce.Application.DTOs.User.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, UserResponse>();

            CreateMap<AppUser, UserDetailResponse>();

            CreateMap<UpdateProfileRequest, AppUser>();
        }
    }
}
