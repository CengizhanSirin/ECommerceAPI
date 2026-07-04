namespace ECommerce.Application.DTOs.Auth.Responses;

public record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiration, DateTimeOffset RefreshTokenExpiration);
