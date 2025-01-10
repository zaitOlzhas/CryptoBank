using System.Security.Claims;
using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Exceptions;
using CryptoBank_WebApi.Features.Account.Configurations;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class CreateAccount
{
    [HttpPost("/create-account")]
    [Authorize]
    public class Endpoint(IMediator mediator) : EndpointWithoutRequest<Response>
    {
        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken)
        {
            var principal = this.HttpContext.User;
            var email = principal.GetClaim(ClaimTypes.Email);
            var request = new Request(email);
            return await mediator.Send(request, cancellationToken);
        }
    }

    public record Request(string? Email) : IRequest<Response>;

    public record Response(AccountModel Account);

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "create_account_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix, dbContext);
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext, IOptions<AccountConfigurations> authConfigs)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email)
                .SingleOrDefaultAsync(cancellationToken);

            var userAccountsCount = await dbContext.Accounts
                .Where(x => x.UserId == user!.Id)
                .CountAsync(cancellationToken);

            if (userAccountsCount >= authConfigs.Value.AccountLimitPerUser)
                throw new LogicConflictException("Account limit reached.", "user_account_limit_reached");

            var account = new Domain.Account()
            {
                Currency = "USD",
                UserId = user!.Id
            };

            var newAccount = await dbContext.Accounts.AddAsync(account, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response(new AccountModel
            {
                Number = newAccount.Entity.Number,
                Currency = newAccount.Entity.Currency,
                Amount = newAccount.Entity.Amount,
                CreatedOn = newAccount.Entity.CreatedOn,
                UserId = newAccount.Entity.UserId
            });
        }
    }
}