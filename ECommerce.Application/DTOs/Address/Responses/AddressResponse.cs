namespace ECommerce.Application.DTOs.Address.Responses;

public sealed class AddressResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string City { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Neighborhood { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}
