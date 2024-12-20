using System.Security.Claims;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Migrations;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class CreateAccount
{
    [HttpPost("/create-account")]
    [Authorize]
    public class Endpoint(IMediator mediator, IHttpContextAccessor contextAccessor) : EndpointWithoutRequest<Response>
    {
        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken) 
        {
            var principal = contextAccessor.HttpContext.User;
            var request = new Request(principal);
            return await mediator.Send(request, cancellationToken);
        }
    }
    public record Request(ClaimsPrincipal Principal) : IRequest<Response>;
    public record Response(AccountModel account);

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (!request.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
                throw new Exception("Invalid user");
            var claims = request.Principal.Claims.ToList();
            var email = claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await dbContext.Users
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync(cancellationToken);
            var account = new Domain.Account();
            account.Currency = "USD";
            account.UserId = user.Id;

            var newAccount = dbContext.Accounts.Add(account);

            return new Response(new AccountModel(
                newAccount.Entity.Number,
                newAccount.Entity.Currency,
                newAccount.Entity.Amount,
                newAccount.Entity.CreatedOn,
                newAccount.Entity.UserId
            ));
        }
    }
}