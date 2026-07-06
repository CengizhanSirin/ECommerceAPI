using AutoMapper;
using ECommerce.Application.DTOs.Auth.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<AppUser, AuthResponse>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());
        }
    }
}
