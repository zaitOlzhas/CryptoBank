using System.Net.Http.Headers;
using CryptoBank_Tests.Errors.Contracts;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;


namespace CryptoBank_Tests.Features.Accounts.Requests;

public class CreateAccountTest(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_be_successful_create_account()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user1@admin.com", new [] { UserRole.User });
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsync("/create-account", null);

        // Assert
        response.EnsureSuccessStatusCode(); 
        var responseString = await response.Content.ReadAsStringAsync();
        var account = JsonConvert.DeserializeObject<AccountModel>(responseString);
        Assert.NotNull(account);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
    [Fact]
    public async Task Should_be_failed_create_account_with_limit_logic_conflict()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user2@admin.com", new [] { UserRole.User });
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsync("/create-account", null);

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        var contract = JsonConvert.DeserializeObject<LogicConflictProblemDetailsContract>(responseString);
        contract.Should().NotBeNull();
        contract.Title.Should().Be("Logic conflict");
        contract.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422");
        contract.Detail.Should().Be("Account limit reached.");
        contract.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        contract.Errors.Should().Be("user_account_limit_reached");
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.ToString());
    }
}