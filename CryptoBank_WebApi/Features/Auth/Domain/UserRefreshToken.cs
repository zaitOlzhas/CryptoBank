namespace CryptoBank_WebApi.Features.Auth.Domain;

public class UserRefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UserId { get; set; }
}