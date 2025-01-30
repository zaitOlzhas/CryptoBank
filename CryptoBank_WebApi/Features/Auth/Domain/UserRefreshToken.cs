namespace CryptoBank_WebApi.Features.Auth.Domain;

public class UserRefreshToken
{
    public int Id { get; set; }
    public required string Token { get; init; }
    public required DateTime ExpiryDate { get; init; }
    public required int UserId { get; init; }
}