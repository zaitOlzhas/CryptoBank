using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Features.Auth.Requests;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.User.Requests;

public class UserProfile
{
    [HttpGet("/user-profile")]
    [AllowAnonymous]
    public class Endpoint(IMediator mediator) : Endpoint<Request,Response>
    {
        public override async Task<Response> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }

    public record Request(string Email) : IRequest<Response>;
    public record Response(UserModel userProfile);
    public class RequestHandler(CryptoBank_DbContext dbContext)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email.ToLower())
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
                ).FirstOrDefaultAsync(cancellationToken);
            
            if (user is null)
                throw new Exception("User not found");
            
            return new Response(user);
        }
    }
}