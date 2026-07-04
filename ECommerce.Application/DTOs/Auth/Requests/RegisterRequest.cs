namespace ECommerce.Application.DTOs.Auth.Requests;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string ConfirmPassword);