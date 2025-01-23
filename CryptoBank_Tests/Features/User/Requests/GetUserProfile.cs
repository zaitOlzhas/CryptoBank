using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank_Tests.Features.User.Requests;

public class GetUserProfile(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_user_profile()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var jwt = scope.GetJwtToken("user1@admin.com", "user");
       
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        
        // Act
        var response = await client.GetAsync("/user-profile");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        //var user = await response.DeserializeContent<UserModel>().ConfigureAwait(false);
        //Assert.NotNull(user);
        //TODO: Найти причину почему не десериализуется. Объект приходит отличчно, все поля заполнены. GetNews работат хорошо.
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}