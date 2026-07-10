using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Address.Requests;
using ECommerce.Application.DTOs.Address.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IAddressService
    {
        Task<ResultT<IReadOnlyList<AddressListResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ResultT<AddressDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ResultT<AddressResponse>> CreateAsync(CreateAddressRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(int id, UpdateAddressRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> SetDefaultAsync(int id, CancellationToken cancellationToken = default);
    }
}
