namespace CryptoBank_WebApi.Features.Auth.Configurations;

public class AuthConfigurations
{
    public JwtOptions Jwt { get; set; }
    public Administator Admin { get; set; }

    public class JwtOptions
    {
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; }
    }
    public class Administator
    {
        public string Email { get; set; }
    }
}