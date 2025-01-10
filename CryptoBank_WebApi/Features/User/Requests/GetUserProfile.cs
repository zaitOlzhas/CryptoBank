using System.Security.Claims;
using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Features.Auth.Requests;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.User.Requests;

public class GetUserProfile
{
    [HttpGet("/user-profile")]
    [Authorize]
    public class Endpoint(IMediator mediator) : EndpointWithoutRequest<Response>
    {
        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken)
        {
            var principal = this.HttpContext.User;
            if (!principal.HasClaim(x => x.Type == ClaimTypes.Email))
                throw new Exception("Missing email from auth token");

            var email = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var request = new Request(email);
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }

    public record Request(string? Email) : IRequest<Response>;

    public record Response(UserModel UserProfile);

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "get_user_profile_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix, dbContext);
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email!.ToLower())
                .Select(x => new UserModel
                    {
                        Id = x.Id,
                        Email = x.Email,
                        Role = x.Role,
                        DateOfBirth = x.DateOfBirth,
                        RegistrationDate = x.RegistrationDate,
                        FirstName = x.FirstName,
                        LastName = x.LastName
                    }
                ).SingleOrDefaultAsync(cancellationToken);

            if (user is null)
                throw new Exception("User not found");

            return new Response(user);
        }
    }
}