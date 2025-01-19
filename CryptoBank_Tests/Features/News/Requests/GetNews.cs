using CryptoBank_WebApi.Database;
using Xunit.Abstractions;
using CryptoBank_WebApi.Features.News.Domain;

namespace CryptoBank_Tests.Features.News.Requests;

public class GetNews: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    private CryptoBank_DbContext _db;
    public GetNews(ITestOutputHelper testOutputHelper, CustomWebApplicationFactory<Program> factory)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
    }
     
    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newsCount = 3;

        // Act
        var response = await client.GetAsync("/allnews").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var news = await response.DeserializeContent<CryptoBank_WebApi.Features.News.Domain.News[]>().ConfigureAwait(false);
        Assert.Equal(newsCount, news.Length);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }
}