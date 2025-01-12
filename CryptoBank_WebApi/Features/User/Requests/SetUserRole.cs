using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Database;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CryptoBank_WebApi.Features.User.Requests;

public class SetUserRole
{
    [HttpPost("/user-role")]
    [Authorize(Policy = PolicyNames.AdministratorRole)]
    public class Endpoint(IMediator mediator) : Endpoint<Request, EmptyResponse>
    {
        public override async Task<EmptyResponse> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await mediator.Send(request, cancellationToken);
    }

    public record Request(int UserId, string Role) : IRequest<EmptyResponse>;

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "set_user_role_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Role)
                .Must(x => Enum.TryParse<Auth.Domain.UserRole>(x, out _))
                .WithErrorCode(MessagePrefix + "role_invalid");
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, EmptyResponse>
    {
        public async Task<EmptyResponse> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FindAsync(request.UserId, cancellationToken);
            if(user is null)
                throw new ValidationException("User with provided id not found.");
            user!.Role = request.Role;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new EmptyResponse();
        }
    }
}