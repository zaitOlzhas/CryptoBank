using System.Net.Http.Headers;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Domain;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.Accounts.Requests;

public class GetAccountReport(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_be_successfull_report()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("analyst@admin.com", new [] { UserRole.Analyst });
        
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync($"/accounts-report?StartDate={DateTime.UtcNow.AddMonths(-3).ToShortDateString()}&EndDate={DateTime.UtcNow.ToShortDateString()}");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        var responseString = await response.Content.ReadAsStringAsync();
        var reportRecords = JsonConvert.DeserializeObject<string[]>(responseString);
        Assert.NotNull(reportRecords);
        Assert.Equal(3, reportRecords.Length);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}