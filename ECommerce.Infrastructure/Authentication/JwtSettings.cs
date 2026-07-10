namespace ECommerce.Infrastructure.Authentication
{
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; set; } = null!;

        public string Issuer { get; set; } = null!;

        public string Audience { get; set; } = null!;

        public int AccessTokenExpirationMinutes { get; set; }

        public int RefreshTokenExpirationDays { get; set; }
    }
}