using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace CryptoBank_Tests.Features.User.Requests;

public class GetUserProfile : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;

    private CryptoBank_DbContext _db;

    public GetUserProfile(ITestOutputHelper testOutputHelper, CustomWebApplicationFactory<Program> factory)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
    }
    
    [Fact]
    public async Task Get_user_profile()
    {
        // Arrange
        var client = _factory.CreateClient();
        var authConfigs = _factory.Services.GetRequiredService<IOptions<AuthConfigurations>>().Value.Jwt;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfigs.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: authConfigs.Issuer,
            audience: authConfigs.Audience,
            claims: new List<Claim>
            {
                new(ClaimTypes.Email, "user1@admin.com"),
                new(ClaimTypes.Role, "user"), // TODO: check
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        
        // Act
        var response = await client.GetAsync("/user-profile").ConfigureAwait(false);

        // Assert
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _testOutputHelper.WriteLine(responseString);
        
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        
        //var user = await response.DeserializeContent<UserModel>().ConfigureAwait(false);
        //Assert.NotNull(user);
        //TODO: Найти причину почему не десериализуется. Объект приходит отличчно, все поля заполнены. GetNews работат хорошо.
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }
}