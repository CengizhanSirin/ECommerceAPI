namespace ECommerce.Application.DTOs.Address.Responses;

public record AddressDetailResponse(int Id, string Title, string FirstName, string LastName, string Phone, string City, string District, string Neighborhood, string Street, string? PostalCode,
    bool IsDefault, DateTimeOffset CreatedDate, DateTimeOffset? UpdatedDate);

