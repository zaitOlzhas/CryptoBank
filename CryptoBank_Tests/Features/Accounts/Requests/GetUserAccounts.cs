using System.Net.Http.Headers;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Domain;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.Accounts.Requests;

public class GetUserAccounts(CustomWebApplicationFactory<Program> factory): IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_be_successful_get_user_accounts()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user2@admin.com", new [] { UserRole.User });
       
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        
        // Act
        var response = await client.GetAsync("/user-accounts");

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        var accounts = JsonConvert.DeserializeObject<AccountModel[]>(responseString);
        response.EnsureSuccessStatusCode(); 
        Assert.NotNull(accounts);
        Assert.Equal(5, accounts.Length);
        //TODO: Найти причину почему не десериализуется. Объект приходит отличчно, все поля заполнены. GetNews работат хорошо.
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}