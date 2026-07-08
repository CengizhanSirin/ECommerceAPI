namespace ECommerce.Application.DTOs.Auth.Responses;

public sealed class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset AccessTokenExpiration { get; set; }
    public DateTimeOffset RefreshTokenExpiration { get; set; }
}
