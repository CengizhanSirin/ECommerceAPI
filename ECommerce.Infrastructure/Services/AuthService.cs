using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Auth.Requests;
using ECommerce.Application.DTOs.Auth.Responses;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(UserManager<AppUser> userManager, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

            if (existingRefreshToken is null)
            {
                return Result.NotFound(AuthMessages.RefreshTokenNotFound);
            }

            if (existingRefreshToken.RevokedAt is not null)
            {
                return Result.Success(AuthMessages.LogoutSuccess);
            }

            existingRefreshToken.RevokedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(AuthMessages.LogoutSuccess);
        }

        public async Task<ResultT<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (existingRefreshToken is null)
            {
                return ResultT<TokenResponse>.Unauthorized(AuthMessages.InvalidRefreshToken);
            }

            if (existingRefreshToken.RevokedAt is not null || existingRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return ResultT<TokenResponse>.Unauthorized(AuthMessages.InvalidRefreshToken);
            }

            var user = existingRefreshToken.AppUser;

            existingRefreshToken.RevokedAt = DateTimeOffset.UtcNow;

            var newToken = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var newRefreshToken = new RefreshToken
            {
                Token = newToken.RefreshToken,
                ExpiresAt = newToken.RefreshTokenExpiration,
                AppUserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ResultT<TokenResponse>.Success(newToken, AuthMessages.RefreshTokenSuccess);
        }

        public async Task<ResultT<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return ResultT<AuthResponse>.Unauthorized(AuthMessages.InvalidEmailOrPassword);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
            {
                return ResultT<AuthResponse>.Unauthorized(AuthMessages.InvalidEmailOrPassword);
            }

            var activeRefreshToken = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);


            if (activeRefreshToken is not null)
            {
                activeRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
            }

            var token = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var refreshToken = new RefreshToken
            {
                Token = token.RefreshToken,
                ExpiresAt = token.RefreshTokenExpiration,
                AppUserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<AuthResponse>(user);
            response.Token = token;

            return ResultT<AuthResponse>.Success(response, AuthMessages.LoginSuccess);
        }

        public async Task<ResultT<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                return ResultT<AuthResponse>.Conflict(AuthMessages.EmailAlreadyExists);
            }

            var user = _mapper.Map<AppUser>(request);

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(x => x.Description).Distinct().ToArray();

                return ResultT<AuthResponse>.Failure(AuthMessages.RegisterFailed, errors: errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Customer);

            if (!roleResult.Succeeded)
            {
                // Kullanıcı oluşturuldu ancak rol eklenemedi.
                // Yarım kayıt bırakmamak için kullanıcıyı geri siliyoruz.
                await _userManager.DeleteAsync(user);

                var errors = roleResult.Errors.Select(x => x.Description).Distinct().ToArray();

                return ResultT<AuthResponse>.Failure(AuthMessages.RoleAssignmentFailed, errors: errors);
            }

            var token = await _jwtService.CreateTokenAsync(user, cancellationToken);

            var refreshToken = new RefreshToken
            {
                Token = token.RefreshToken,
                ExpiresAt = token.RefreshTokenExpiration,
                AppUserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<AuthResponse>(user);
            response.Token = token;

            return ResultT<AuthResponse>.Success(response, AuthMessages.RegisterSuccess);
        }
    }
}
