using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank_Tests.Features.User.Requests;

public class SetUserRole(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task SetUserRole_ShouldBeSuccessful()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();

        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("admin@admin.com",  [UserRole.Administrator]);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/user-role", new
        {
            UserId= 4,
            Role = "Analyst"
        });

        // Assert
        httpResponse.EnsureSuccessStatusCode();
        var db = scope.ServiceProvider.GetRequiredService<CryptoBank_DbContext>();
        var user = db.Users.Find(4);
        user.Should().NotBeNull();
        user.Role.Should().Be("Analyst");
    }
    
    [Fact]
    public async Task SetUserRole_ShouldFail_WhenForbidden()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();

        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user1@admin.com", [UserRole.User]);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/user-role", new
        {
            UserId= 4,
            Role= "Analyst"
        });

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}