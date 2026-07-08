namespace ECommerce.Application.DTOs.Address.Responses;

public sealed class AddressListResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string City { get; set; } = null!;
    public string District { get; set; } = null!;
    public bool IsDefault { get; set; }
}
