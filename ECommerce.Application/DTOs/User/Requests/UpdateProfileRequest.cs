namespace ECommerce.Application.DTOs.User.Requests;

public record UpdateProfileRequest(string FirstName, string LastName, string? PhoneNumber);