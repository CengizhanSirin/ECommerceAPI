namespace ECommerce.Application.DTOs.Address.Requests;

public record CreateAddressRequest(string Title, string FirstName, string LastName, string Phone, string City, string District, string Neighborhood, string Street, string? PostalCode, bool IsDefault);