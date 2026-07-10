using ECommerce.Application.DTOs.Auth.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IJwtService
    {
        Task<TokenResponse> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
    }
}
