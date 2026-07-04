namespace ECommerce.Application.DTOs.User.Responses;

public record UserDetailResponse(int Id, string FirstName, string LastName, string Email, string? PhoneNumber, DateTimeOffset? LockoutEnd, bool EmailConfirmed);