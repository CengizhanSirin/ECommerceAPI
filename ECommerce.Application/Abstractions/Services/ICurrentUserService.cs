namespace ECommerce.Application.Abstractions.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }

        string? Email { get; }

        bool IsAuthenticated { get; }
    }
}
