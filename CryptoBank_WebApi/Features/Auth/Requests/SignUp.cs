using CryptoBank_WebApi.Common.Extensions;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.Auth.Requests;

public class SignUp
{
    [HttpPost("/signup")]
    [AllowAnonymous]
    public class Endpoint(IMediator mediator) : Endpoint<Request, EmptyResponse>
    {
        public override async Task<EmptyResponse> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(request, cancellationToken);
            HttpContext.Response.StatusCode = 201;
            return response;
        }
    }

    public record Request(string Email, string Password, DateOnly DateOfBirth) : IRequest<EmptyResponse>;

    public record EmptyResponse();

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "sign_up_validation_";

        public RequestValidator(CryptoBank_DbContext dbContext)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(MessagePrefix + "email_empty");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessagePrefix + "password_empty");
        }
    }

    public class RequestHandler(
        CryptoBank_DbContext dbContext,
        IOptions<AuthConfigurations> authConfigs,
        Argon2IdPasswordHasher passwordHasher)
        : IRequestHandler<Request, EmptyResponse>
    {
        private readonly AuthConfigurations _authConfigs = authConfigs.Value;

        public async Task<EmptyResponse> Handle(Request request, CancellationToken cancellationToken)
        {
            ////TODO: Надо ли проверять конфликт в валидаторе?
            var user = await dbContext.Users
                .Where(x => x.Email == request.Email.ToLower())
                .AnyAsync(cancellationToken);

            if (user)
                throw new Exception("This email is already in use!");

            var role = _authConfigs.Admin.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
                ? UserRole.Administrator
                : UserRole.User;
            var userEntity = new Domain.User
            {
                Email = request.Email.ToLower(),
                Password = passwordHasher.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                RegistrationDate = DateTime.Now.SetKindUtc(),
                Role = role.ToString()
            };

            await dbContext.Users.AddAsync(userEntity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new EmptyResponse();
        }
    }
}