using System.Security.Claims;
using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.News.Models;
using CryptoBank_WebApi.Features.News.Requests;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class GetUserAccounts
{
    [HttpGet("/user-accounts")]
    [Authorize]
    public class Endpoint(IMediator mediator, IHttpContextAccessor contextAccessor) : EndpointWithoutRequest<AccountModel[]>
    {
        public override async Task<AccountModel[]> ExecuteAsync(CancellationToken cancellationToken)
        {
            var principal = contextAccessor.HttpContext!.User;
            var request = new GetAccountsRequest(principal);
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }
    public record GetAccountsRequest(ClaimsPrincipal Principal) : IRequest<AccountModel[]>;

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<GetAccountsRequest, AccountModel[]>
    {
        public async Task<AccountModel[]> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
        {
            if (!request.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
                throw new Exception("Invalid user");
            var claims = request.Principal.Claims.ToList();
            var email = claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await dbContext.Users
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                throw new Exception("Invalid user");

            var userAccounts = await dbContext.Accounts
                .Where(x => x.UserId == user.Id)
                .Select(x => new AccountModel
                {
                    Number = x.Number,
                    Currency = x.Currency,
                    Amount = x.Amount,
                    UserId = x.UserId,
                    CreatedOn = x.CreatedOn
                })
                .ToListAsync(cancellationToken);
            return userAccounts.ToArray();
        }
    }
}