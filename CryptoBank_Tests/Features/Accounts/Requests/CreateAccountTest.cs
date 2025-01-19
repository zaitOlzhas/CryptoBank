using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Features.News.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace CryptoBank_Tests.Features.Accounts.Requests;

public class CreateAccountTest: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    private CryptoBank_DbContext _db;
    public CreateAccountTest(ITestOutputHelper testOutputHelper, CustomWebApplicationFactory<Program> factory)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
    }
    [Fact]
    public async Task Post_create_account()
    {
        // Arrange
        var _scope = _factory.Services.CreateAsyncScope();
        var _authConfigs = _scope.ServiceProvider.GetRequiredService<IOptions<AuthConfigurations>>().Value.Jwt;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfigs.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _authConfigs.Issuer,
            audience: _authConfigs.Audience,
            claims: new List<Claim>
            {
                new(ClaimTypes.Email, "user1@admin.com"),
                new(ClaimTypes.Role, "user"), // TODO: check
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsync("/create-account", null);

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(response.ToString());
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        //var account = await response.DeserializeContent<AccountModel>();
        //TODO: Найти причину почему не десериализуется. Счет генерится в базе, надо придумать как это обойти. или проверять без номера счета
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }
}