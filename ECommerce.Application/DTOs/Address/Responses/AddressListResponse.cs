namespace ECommerce.Application.DTOs.Address.Responses;

public record AddressListResponse(int Id, string Title, string City, string District, bool IsDefault);
