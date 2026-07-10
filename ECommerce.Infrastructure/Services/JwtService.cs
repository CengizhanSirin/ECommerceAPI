using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.DTOs.Auth.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Infrastructure.Services
{
    public sealed class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(IOptions<JwtSettings> jwtOptions, UserManager<AppUser> userManager)
        {
            _jwtSettings = jwtOptions.Value;
            _userManager = userManager;
        }
        public async Task<TokenResponse> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            var accessTokenExpiration = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            var refreshTokenExpiration = now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            var claims = new List<Claim>
            {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };


            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: accessTokenExpiration.UtcDateTime,
                signingCredentials: signingCredentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };

        }
    }
}
