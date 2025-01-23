namespace CryptoBank_Tests.Features.News.Requests;

public class GetNews(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        // Arrange
        var client = factory.CreateClient();
        var newsCount = 3;

        // Act
        var response = await client.GetAsync("/allnews");

        // Assert
        response.EnsureSuccessStatusCode(); 

        var news = await response.DeserializeContent<CryptoBank_WebApi.Features.News.Domain.News[]>();
        Assert.Equal(newsCount, news.Length);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}