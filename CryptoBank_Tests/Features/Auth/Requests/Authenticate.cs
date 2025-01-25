using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CryptoBank_Tests.Errors.Contracts;
using CryptoBank_Tests.Features.Auth.Contracts;
using CryptoBank_WebApi.Features.Auth.Configurations;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.Auth.Requests;

public class Authenticate(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_be_successful_authentication()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var client = factory.CreateClient();
        var authObject = new
        {
            Email = "admin@admin.com",
            Password = "123"
        };
        // Act
        var response = await client.PostAsJsonAsync("/auth", authObject);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        
        var contract  = JsonConvert.DeserializeObject<AccessTokenContract>(responseString);
        contract.Should().NotBeNull();
        contract.Jwt.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var authConfigurations = scope.ServiceProvider.GetRequiredService<IOptions<AuthConfigurations>>().Value;
        var key = authConfigurations.Jwt.SigningKey;
        tokenHandler.ValidateToken(contract.Jwt, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = authConfigurations.Jwt.Issuer,
            ValidAudience = authConfigurations.Jwt.Audience
        }, out var validatedToken);
        
        validatedToken.Should().NotBeNull();
        var jwtToken = (JwtSecurityToken)validatedToken;
        var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;

        email.Should().Be(authObject.Email);
    }
    [Fact]
    public async Task Should_be_failed_authentication_with_user_not_found_validation_error()
    {
        // Arrange
        var client = factory.CreateClient();
        var authObject = new
        {
            Email = "notuser@admin.com",
            Password = "123"
        };
        // Act
        var response = await client.PostAsJsonAsync("/auth", authObject);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseString = await response.Content.ReadAsStringAsync();
        
        var contract  = JsonConvert.DeserializeObject<ValidationProblemDetailsContract>(responseString);
        contract.Should().NotBeNull();
        contract.Title.Should().Be("Validation failed");
        contract.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400");
        contract.Detail.Should().Be("One or more validation errors have occurred");
        contract.Errors.Should().HaveCount(1);

        var error = contract.Errors.Single();
        error.Field.Should().Be("Email");
        error.Message.Should().Be("User not found by given email.");
        error.Code.Should().Be("authenticate_user_email_not_found");
    }
}