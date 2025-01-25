using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CryptoBank_Tests.Errors.Contracts;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CryptoBank_Tests.Features.Accounts.Requests;

public class MoneyTransfer(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task MoneyTransfer_ShouldBeSuccessful()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
       
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user1@admin.com", new [] { UserRole.User });
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
       
        var sourceAccount = new 
        {
            Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da83",
            Amount = 98
        };
        var destinationAccount = new 
        {
            Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da84",
            Amount = 1,
        };
        var amount = 12;

        // Act
        var httpResponse = await client.PostAsJsonAsync("/money-transfer", new
        {
            SourceAccountNumber = sourceAccount.Number,
            DestinationAccountNumber = destinationAccount.Number,
            Amount = amount
        });

        // Assert
        httpResponse.EnsureSuccessStatusCode();
        var db2 = scope.ServiceProvider.GetRequiredService<CryptoBank_DbContext>();
        var moneyTransaction = db2.MoneyTransactions.Where(x=>x.SourceAccount==sourceAccount.Number && x.DestinationAccount== destinationAccount.Number).OrderByDescending(a=>a.CreatedOn).SingleOrDefault();
        moneyTransaction.Should().NotBeNull();
        moneyTransaction.Amount.Should().Be(amount);
        var sourceAccountResult = db2.Accounts.SingleOrDefault(x => x.Number == sourceAccount.Number);
        var destinationAccountResult = db2.Accounts.SingleOrDefault(x => x.Number == destinationAccount.Number);
        sourceAccountResult!.Amount.Should().Be(sourceAccount.Amount - amount);
        destinationAccountResult!.Amount.Should().Be(destinationAccount.Amount + amount);
    }

    [Fact]
    public async Task MoneyTransfer_ShouldReturnValidationError_WhenSourceAccountNotFound()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
        
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user1@admin.com", new [] { UserRole.User });
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
    public async Task MoneyTransfer_ShouldReturnValidationError_WhenBothAccountsNotFound()
    {
        // Arrange
        var scope = factory.Services.CreateAsyncScope();
       
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<TokenGenerator>();
        var jwt = tokenGenerator.GenerateJwt("user1@admin.com", new [] { UserRole.User });
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
        var contract = JsonConvert.DeserializeObject<ValidationProblemDetailsContract>(content);
        
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