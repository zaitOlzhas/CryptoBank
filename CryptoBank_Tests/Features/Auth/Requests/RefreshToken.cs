using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CryptoBank_Tests.Errors.Contracts;
using CryptoBank_Tests.Features.Auth.Contracts;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.Auth.Requests;

public class RefreshToken(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task RefreshToken_ShouldBeSuccessful()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();

        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var authObject = new 
        {
            Email = "admin@admin.com",
            Role = new [] { UserRole.Administrator },
            RefreshToken ="eededyJD9w8icqFkiVFtn7Kt+DwFLrEEJ1OoSfTqkn0="
        };
        var jwt = tokenGenerator.GenerateJwt(authObject.Email, authObject.Role);
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/refresh-token");
        httpRequestMessage.Headers.Add("Cookie", $"RefreshToken={authObject.RefreshToken};");
        
        // Act
        var httpResponse = await client.SendAsync(httpRequestMessage);
        
        // Assert
        httpResponse.EnsureSuccessStatusCode();
        var responseString = await httpResponse.Content.ReadAsStringAsync();
        var responseCookie = httpResponse.Headers.GetValues(HeaderNames.SetCookie).First();
        
        var token = responseCookie.Split(";").First().Split("=").Last();
        token = WebUtility.UrlDecode(token);
        var db = scope.ServiceProvider.GetRequiredService<CryptoBank_DbContext>();
        var newTokeCheck = db.UserRefreshTokens.SingleOrDefault(x => x.Token == token);
        newTokeCheck.Should().NotBeNull();
        newTokeCheck.Should().NotBe(authObject.RefreshToken);
        
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
    public async Task RefreshToken_ShouldReturnValidationError_WhenRefreshTokenIsMissing()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();

        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var authObject = new 
        {
            Email = "admin@admin.com",
            Role = new [] { UserRole.Administrator },
            RefreshToken ="eededyJD9w8icqFkiVFtn7Kt+DwFLrEEJ1OoSfTqkn0="
        };
        var jwt = tokenGenerator.GenerateJwt(authObject.Email, authObject.Role);
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/refresh-token");
        
        // Act
        var response = await client.SendAsync(httpRequestMessage);
        
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
        error.Field.Should().Be("RefreshToken");
        error.Message.Should().Be("Missing refresh token");
        error.Code.Should().Be("refresh_token_validation_refresh_token_empty");
    }
    
    [Fact]
    public async Task RefreshToken_ShouldReturnValidationError_WhenEmailClaimIsEmpty()
    {
        // Arrange
        var authObject = new 
        {
            RefreshToken ="eededyJD9w8icqFkiVFtn7Kt+DwFLrEEJ1OoSfTqkn0="
        };
        
        var client = factory.CreateClient();
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/refresh-token");
        httpRequestMessage.Headers.Add("Cookie", $"RefreshToken={authObject.RefreshToken};");
        
        // Act
        var response = await client.SendAsync(httpRequestMessage);
        
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
        error.Message.Should().Be("Missing email from auth token");
        error.Code.Should().Be("refresh_token_validation_email_empty");
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnValidationError_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();

        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var authObject = new 
        {
            Email = "admin@admin.com",
            Role = new [] { UserRole.Administrator },
            RefreshToken ="wrong_token"
        };
        var jwt = tokenGenerator.GenerateJwt(authObject.Email, authObject.Role);
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/refresh-token");
        httpRequestMessage.Headers.Add("Cookie", $"RefreshToken={authObject.RefreshToken};");
        
        // Act
        var response = await client.SendAsync(httpRequestMessage);
        
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
        error.Field.Should().Be("RefreshToken");
        error.Message.Should().Be("Refresh token not found.");
        error.Code.Should().Be("refresh_token_validation_token_not_found");
    }
}