using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.Auth.Requests;

public class SignUp
{
    [HttpPost("/signup")]
    [AllowAnonymous]
    public class Endpoint(IMediator mediator) : Endpoint<Request,EmptyResponse>
    {
        public override async Task<EmptyResponse> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(request, cancellationToken);
            HttpContext.Response.StatusCode = 201;
            //TODO: Consider handling response codes in handler
            return response;
        }
    }

    public record Request(string Email, string Password) : IRequest<EmptyResponse>;
    public record EmptyResponse();
    public class RequestHandler(CryptoBank_DbContext dbContext, IOptions<AuthConfigurations> authConfigs)
        : IRequestHandler<Request, EmptyResponse>
    {
        private readonly AuthConfigurations _authConfigs = authConfigs.Value;

        public async Task<EmptyResponse> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email)
                .Select(x => new UserModel
                    {
                        Id = x.Id,
                        Email = x.Email,
                        Role = x.Role,
                    }
                )
                .FirstOrDefaultAsync(cancellationToken);
            
            if (user != null)
                throw new Exception("This email is already in use!");
            //TODO: Add http code 409 for conflict emails
            
            var role = _authConfigs.Admin.Email.Equals(request.Email) ? UserRole.Administrator : UserRole.User;
            var userEntity = new User
            {
                Email = request.Email,
                Password = request.Password,
                Role = role.ToString()
            };
            
            await dbContext.Users.AddAsync(userEntity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return await Task.FromResult(new EmptyResponse());
        }
    }
}