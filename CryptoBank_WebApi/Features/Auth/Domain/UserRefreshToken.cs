using System.ComponentModel.DataAnnotations;

namespace CryptoBank_WebApi.Features.Auth.Domain;

public class UserRefreshToken
{
    public int Id { get; set; }
    [Required]
    public string Token { get; init; }
    [Required]
    public DateTime ExpiryDate { get; init; }
    [Required]
    public int UserId { get; init; }
}