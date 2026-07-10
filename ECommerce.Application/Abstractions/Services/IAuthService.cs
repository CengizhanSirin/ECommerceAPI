using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Auth.Requests;
using ECommerce.Application.DTOs.Auth.Responses;

namespace ECommerce.Application.Abstractions.Services
{
    public interface IAuthService
    {
        Task<ResultT<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<ResultT<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ResultT<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
