using System.Text.Json.Serialization;

namespace CryptoBank_WebApi.Features.Auth.Model;

public class UserModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
    public string Role { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}