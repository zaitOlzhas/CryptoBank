using System.Net.Http.Headers;
using CryptoBank_WebApi.Features.Auth.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
        var responseString = await response.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<UserModel>(responseString);
        Assert.NotNull(user);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}