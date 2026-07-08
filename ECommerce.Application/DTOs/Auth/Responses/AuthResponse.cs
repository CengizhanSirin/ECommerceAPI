namespace ECommerce.Application.DTOs.Auth.Responses;

public sealed class AuthResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public TokenResponse Token { get; set; } = null!;
}