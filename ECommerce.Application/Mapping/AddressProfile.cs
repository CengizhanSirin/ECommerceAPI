using AutoMapper;
using ECommerce.Application.DTOs.Address.Requests;
using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping
{
    public class AddressProfile:Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, AddressResponse>();

            CreateMap<Address, AddressListResponse>();

            CreateMap<Address, AddressDetailResponse>();

            CreateMap<CreateAddressRequest, Address>();

            CreateMap<UpdateAddressRequest, Address>();
        }
    }
}
