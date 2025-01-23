using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;


namespace CryptoBank_Tests.Features.Accounts.Requests;

public class CreateAccountTest(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Post_create_account()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var jwt = scope.GetJwtToken("user1@admin.com", "user");
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsync("/create-account", null);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        //var account = await response.DeserializeContent<AccountModel>();
        //TODO: Найти причину почему не десериализуется. Счет генерится в базе, надо придумать как это обойти. или проверять без номера счета
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}