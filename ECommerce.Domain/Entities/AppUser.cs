using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }



        public ICollection<Address> Addresses { get; set; } = [];

        public ICollection<Order> Orders { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

       
        public Cart? Cart { get; set; }
    }
}
