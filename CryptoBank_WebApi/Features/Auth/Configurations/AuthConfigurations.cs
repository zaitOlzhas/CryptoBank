namespace CryptoBank_WebApi.Features.Auth.Configurations;

public class AuthConfigurations
{
    public required JwtOptions Jwt { get; set; }
    public required Administator Admin { get; set; }

    public class JwtOptions
    {
        public required string SigningKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public TimeSpan Expiration { get; set; }
        public TimeSpan RefreshTokenExpiration { get; set; }
    }
    public class Administator
    {
        public required string Email { get; set; }
    }
}