namespace CryptoBank_WebApi.Features.Auth.Domain;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}