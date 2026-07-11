using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.User.Requests;
using ECommerce.Application.DTOs.User.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task<ResultT<UserDetailResponse>> GetProfileAsync(CancellationToken cancellationToken = default);

        Task<ResultT<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    }
}
