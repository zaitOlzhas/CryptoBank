using System.Security.Claims;
using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Configurations;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Migrations;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class CreateAccount
{
    [HttpPost("/create-account")]
    [Authorize]
    public class Endpoint(IMediator mediator, IHttpContextAccessor contextAccessor) : EndpointWithoutRequest<Response>
    {
        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken) 
        {
            var principal = contextAccessor.HttpContext!.User;
            var request = new Request(principal);
            return await mediator.Send(request, cancellationToken);
        }
    }
    public record Request(ClaimsPrincipal Principal) : IRequest<Response>;
    public record Response(AccountModel Account);

    public class RequestHandler(CryptoBank_DbContext dbContext, IOptions<AccountConfigurations> authConfigs)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var email = request.Principal.GetClaim(ClaimTypes.Email);
            if(string.IsNullOrWhiteSpace(email)) throw new Exception("Invalid user");
            var user = await dbContext.Users
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                throw new Exception("Invalid user");

            var userAccountsCount = await dbContext.Accounts
                .Where(x => x.UserId == user.Id)
                .CountAsync(cancellationToken);

            if (userAccountsCount >= authConfigs.Value.AccountLimitPerUser)
                throw new Exception("Account limit reached");

            var account = new Domain.Account()
            {
                Currency = "USD",
                UserId = user.Id
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