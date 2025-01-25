using System.Net;
using FluentAssertions;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.News.Requests;

public class GetNews(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetNews_ShouldBeSuccessful()
    {
        // Arrange
        var client = factory.CreateClient();
        var newsCount = 3;

        // Act
        var response = await client.GetAsync("/allnews");

        // Assert
        response.EnsureSuccessStatusCode(); 
        var responseString = await response.Content.ReadAsStringAsync();
        var news = JsonConvert.DeserializeObject<CryptoBank_WebApi.Features.News.Domain.News[]>(responseString);
        Assert.Equal(newsCount, news?.Length);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
    
    [Fact]
    public async Task GetNews_ShouldFail_WhenForbidden()
    {
        // Arrange
        var client = factory.CreateClient();
        // Act
        var httpResponse = await client.GetAsync("/usernews");
        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}