using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CryptoBank_Tests.Errors.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank_Tests.Features.Accounts.Requests;

public class MoneyTransfer(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_be_successful_money_transfer()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
       
        var jwt = scope.GetJwtToken("user1@admin.com", "user");

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var amount = 12;

        // Act
        var httpResponse = await client.PostAsJsonAsync("/money-transfer", new
        {
            SourceAccountNumber = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da8a",
            DestinationAccountNumber = "f78d6b07-2d33-4443-91f1-d679a2b80b43",
            Amount = amount
        });

        // Assert
        httpResponse.EnsureSuccessStatusCode();
        //var resultAccount = db.Accounts.Where(x => x.Number == sourceAccount.Number).SingleOrDefault();
        //resultAccount.Amount.Should().Be(sourceAccount.Amount - amount);
    }

    [Fact]
    public async Task Should_be_only_one_source_account_not_found_validation_error()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        
        var jwt = scope.GetJwtToken("user1@admin.com", "user");

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var destinationAccount = "f78d6b07-2d33-4443-91f1-d679a2b80b43";
        var sourceAccount = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da8b"; //wrong account number
        var amount = 12;

        // Act
        var httpResponse = await client.PostAsJsonAsync("/money-transfer", new
        {
            SourceAccountNumber = sourceAccount,
            DestinationAccountNumber = destinationAccount,
            Amount = amount
        });

        // Assert
        var content = await httpResponse.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<ValidationProblemDetailsContract>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        httpResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        contract.Should().NotBeNull();
        contract.Title.Should().Be("Validation failed");
        contract.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400");
        contract.Detail.Should().Be("One or more validation errors have occurred");
        contract.Status.Should().Be(StatusCodes.Status400BadRequest);
        contract.Errors.Should().HaveCount(1);

        var error = contract.Errors.Single();
        error.Field.Should().Be("sourceAccountNumber");
        error.Message.Should().Be("The specified condition was not met for 'source Account Number'.");
        error.Code.Should().Be("money_transfer_validation_source_account_number_does_not_exist");
    }

    [Fact]
    public async Task Should_be_two_account_not_found_validation_error()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
       
        var jwt = scope.GetJwtToken("user1@admin.com", "user");

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var destinationAccount = "f78d6b07-2d33-4443-91f1-d679a2b80b41"; //wrong account number
        var sourceAccount = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da81"; //wrong account number
        var amount = 12;

        // Act
        var httpResponse = await client.PostAsJsonAsync("/money-transfer", new
        {
            SourceAccountNumber = sourceAccount,
            DestinationAccountNumber = destinationAccount,
            Amount = amount
        });

        // Assert
        var content = await httpResponse.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<ValidationProblemDetailsContract>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        httpResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        contract.Should().NotBeNull();
        contract.Title.Should().Be("Validation failed");
        contract.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400");
        contract.Detail.Should().Be("One or more validation errors have occurred");
        contract.Status.Should().Be(StatusCodes.Status400BadRequest);
        contract.Errors.Should().HaveCount(2);

        var error1 = contract.Errors[0];
        error1.Field.Should().Be("sourceAccountNumber");
        error1.Message.Should().Be("The specified condition was not met for 'source Account Number'.");
        error1.Code.Should().Be("money_transfer_validation_source_account_number_does_not_exist");

        var error2 = contract.Errors[1];
        error2.Field.Should().Be("destinationAccountNumber");
        error2.Message.Should().Be("The specified condition was not met for 'destination Account Number'.");
        error2.Code.Should().Be("money_transfer_validation_destination_account_number_does_not_exist");
    }
}