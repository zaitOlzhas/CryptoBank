using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Domain;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.News.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank_Tests;

public static class Extensions
{
    private static object _customerContextLock = new object();
    public static CryptoBank_DbContext InitializeTestDatabase(this CryptoBank_DbContext context)
    {
        lock (_customerContextLock)
        {
            context.News.Add(new News
            {
                Id = 1,
                Title = "TEST 1",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 1"
            });

            context.News.Add(new News
            {
                Id = 2,
                Title = "TEST 2",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 2"
            });

            context.News.Add(new News
            {
                Id = 3,
                Title = "TEST 3",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 3"
            });

            context.Users.Add(new User
            {
                Id = 1,
                Email = "user1@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });

            context.Users.Add(new User
            {
                Id = 2,
                Email = "user2@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });

            context.Users.Add(new User
            {
                Id = 3,
                Email = "user3@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b43",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b49",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b48",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b47",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b46",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow
            });
            context.Accounts.Add(new Account
            {
                Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da8a",
                Currency = "USD",
                UserId = 1,
                Amount = 98,
                CreatedOn = DateTime.UtcNow
            });
            context.SaveChanges();
            return context;
        }
    }
    
    public static string GetJwtToken(this IServiceScope scope, string email, string role)
    {
        var authConfigs = scope.ServiceProvider.GetRequiredService<IOptions<AuthConfigurations>>().Value.Jwt;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfigs.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: authConfigs.Issuer,
            audience: authConfigs.Audience,
            claims: new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role), // TODO: check
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}