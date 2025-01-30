using System.Security.Claims;
using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Exceptions;
using CryptoBank_WebApi.Features.Account.Domain;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class MoneyTransfer
{
    [HttpPost("/money-transfer")]
    [Authorize]
    public class Endpoint(IMediator mediator) : Endpoint<EndPointRequest, Response>
    {
        public override async Task<Response> ExecuteAsync(EndPointRequest request, CancellationToken ct)
        {
            var principal = this.HttpContext.User;
            var email = principal.GetClaim(ClaimTypes.Email);
            var cqrsRequest = new Request
            {
                SourceAccountNumber = request.SourceAccountNumber,
                DestinationAccountNumber = request.DestinationAccountNumber,
                Amount = request.Amount,
                Email = email
            };
            return await mediator.Send(cqrsRequest, ct);
        }
    }

    public record EndPointRequest
    {
        public required string SourceAccountNumber { get; set; }
        public required string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }

    public record Request : IRequest<Response>
    {
        public required string SourceAccountNumber { get; init; }
        public required string DestinationAccountNumber { get; init; }
        public required decimal Amount { get; init; }
        public required string? Email { get; init; }
    }

    public record Response;

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "money_transfer_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix);
            RuleFor(x => x.SourceAccountNumber)
                .ValidateAccountNumber(MessagePrefix + "source_", dbContext);
            RuleFor(x => x.DestinationAccountNumber)
                .ValidateAccountNumber(MessagePrefix + "destination_", dbContext);
            RuleFor(x => x.Amount)
                .ValidateAmount(MessagePrefix);
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email!.ToLower())
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
                throw new ValidationErrorsException(nameof(request.Email), "User not found by given email.","money_transfer_validation_user_not_found");

            var sourceAccount = await dbContext.Accounts.FindAsync(request.SourceAccountNumber, cancellationToken);

            if (sourceAccount!.UserId != user.Id)
                throw new Exception("You are not the owner of the source account");

            var destinationAccount =
                await dbContext.Accounts.FindAsync(request.DestinationAccountNumber, cancellationToken);

            if (sourceAccount.Amount < request.Amount)
                throw new Exception("Insufficient funds");
            if (sourceAccount.Currency != destinationAccount!.Currency)
                throw new Exception("Currency mismatch");

            sourceAccount.Amount -= request.Amount;
            destinationAccount.Amount += request.Amount;

            var newTransaction = new MoneyTransaction
            {
                SourceAccount = sourceAccount.Number,
                DestinationAccount = destinationAccount.Number,
                Amount = request.Amount,
                CreatedOn = DateTime.UtcNow
            };

            await dbContext.MoneyTransactions.AddAsync(newTransaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response();
        }
    }
}