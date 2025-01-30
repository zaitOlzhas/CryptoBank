using System.Net;
using System.Net.Http.Json;
using CryptoBank_Tests.Errors.Contracts;
using CryptoBank_WebApi.Database;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CryptoBank_Tests.Features.Auth.Requests;

public class SignUp(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task SignUp_ShouldBeSuccessful()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        var client = factory.CreateClient();
        var signUpObject = new
        {
            Email = "newuser@admin.com",
            Password = "123",
            DateOfBirth = new DateOnly(2000, 01, 31)
        };
        // Act
        var response = await client.PostAsJsonAsync("/signup", signUpObject);

        // Assert
        response.EnsureSuccessStatusCode();
        var db = scope.ServiceProvider.GetRequiredService<CryptoBank_DbContext>();
        var user = db.Users.SingleOrDefault(x => x.Email == signUpObject.Email);
        user.Should().NotBeNull();
    }
    [Fact]
    public async Task SignUp_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var client = factory.CreateClient();
        var signUpObject = new
        {
            Email = "admin@admin.com",
            Password = "123",
            DateOfBirth = new DateOnly(2000, 01, 31)
        };
        // Act
        var response = await client.PostAsJsonAsync("/signup", signUpObject);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseString = await response.Content.ReadAsStringAsync();
        var contract  = JsonConvert.DeserializeObject<ValidationProblemDetailsContract>(responseString);
        contract.Should().NotBeNull();
        contract.Title.Should().Be("Validation failed");
        contract.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400");
        contract.Detail.Should().Be("One or more validation errors have occurred");
        contract.Errors.Should().HaveCount(1);

        var error = contract.Errors.Single();
        error.Field.Should().Be("Email");
        error.Message.Should().Be("This email is already in use!");
        error.Code.Should().Be("sign_up_validation_email_conflict");
    }
}