namespace CryptoBank_WebApi.Features.Auth.Domain;

public class AuthToken
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}