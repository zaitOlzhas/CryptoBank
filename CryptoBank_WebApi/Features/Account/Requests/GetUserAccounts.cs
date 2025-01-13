using System.Security.Claims;
using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Exceptions;
using CryptoBank_WebApi.Features.Account.Model;
using CryptoBank_WebApi.Features.News.Models;
using CryptoBank_WebApi.Features.News.Requests;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class GetUserAccounts
{
    [HttpGet("/user-accounts")]
    [Authorize]
    public class Endpoint(IMediator mediator) : EndpointWithoutRequest<AccountModel[]>
    {
        public override async Task<AccountModel[]> ExecuteAsync(CancellationToken cancellationToken)
        {
            var principal = this.HttpContext.User;
            var email = principal.GetClaim(ClaimTypes.Email);
            var request = new GetAccountsRequest(email);
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }

    public record GetAccountsRequest(string? Email) : IRequest<AccountModel[]>;

    public class RequestValidator : AbstractValidator<GetAccountsRequest>
    {
        private const string MessagePrefix = "get_user_accounts_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix);
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<GetAccountsRequest, AccountModel[]>
    {
        public async Task<AccountModel[]> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email!.ToLower())
                .SingleOrDefaultAsync(cancellationToken);
            
            if (user is null)
                throw new ValidationErrorsException(nameof(request.Email), "User not found by given email.","get_user_accounts_validation_user_not_found");

            var userAccounts = await dbContext.Accounts
                .Where(x => x.UserId == user!.Id)
                .Select(x => new AccountModel
                {
                    Number = x.Number,
                    Currency = x.Currency,
                    Amount = x.Amount,
                    UserId = x.UserId,
                    CreatedOn = x.CreatedOn
                })
                .ToArrayAsync(cancellationToken);
            return userAccounts;
        }
    }
}