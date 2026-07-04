namespace ECommerce.Application.DTOs.Auth.Responses;

public record AuthResponse(int Id, string FirstName, string LastName, string Email, TokenResponse Token);