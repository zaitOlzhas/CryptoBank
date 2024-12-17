namespace CryptoBank_WebApi.Features.Auth.Domain;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}