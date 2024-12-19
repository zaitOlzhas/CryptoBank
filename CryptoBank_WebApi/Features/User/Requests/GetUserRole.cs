using System.ComponentModel.DataAnnotations;
using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Model;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CryptoBank_WebApi.Features.User.Requests;

public class GetUserRole
{
    [HttpPost("/user-role")]
    [Authorize(Policy = PolicyNames.AdministratorRole)]
    public class Endpoint(IMediator mediator) : Endpoint<Request, Response>
    {
        public override async Task<Response> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await mediator.Send(request, cancellationToken);
    }

    public record Request(int UserId, string Role) : IRequest<Response>;
    public record Response();

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<CryptoBank_WebApi.Features.Auth.Domain.UserRole>(request.Role, out var role))
            {
                throw new ValidationException("Invalid role");
            }
            var user = await dbContext.Users.FindAsync(request.UserId);
            if (user is null)
                throw new Exception("User not found");
            
            user.Role = role.ToString();
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response();
        }
    }
}