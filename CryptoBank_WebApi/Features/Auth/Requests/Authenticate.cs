using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Exceptions;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank_WebApi.Features.Auth.Requests;

public class Authenticate
{
    [HttpPost("/auth")]
    [AllowAnonymous]
    public class Endpoint(
        IMediator mediator,
        IOptions<AuthConfigurations> authConfigs)
        : Endpoint<Request, EndpointResponse>
    {
        public override async Task<EndpointResponse> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(request, cancellationToken);

            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.Add(authConfigs.Value.Jwt.RefreshTokenExpiration)
            };
            this.HttpContext.Response.Cookies.Append("RefreshToken", response.Token, cookie);

            return new EndpointResponse(response.Jwt);
        }
    }

    public record Request(string Email, string Password) : IRequest<Response>;

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "authenticate_validation_";

        public RequestValidator()
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix);
        }
    }

    public record Response(string Jwt, string Token);

    public record EndpointResponse(string Jwt);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CryptoBank_DbContext _dbContext;
        private readonly Argon2IdPasswordHasher _paswordHasher;
        private readonly TokenGenerator _jwtTokenGenerator;

        public RequestHandler(CryptoBank_DbContext dbContext, Argon2IdPasswordHasher paswordHasher,
            TokenGenerator jwtTokenGenerator)
        {
            _dbContext = dbContext;
            _paswordHasher = paswordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Where(x => x.Email == request.Email.ToLower())
                .Select(x => new UserModel
                    {
                        Id = x.Id,
                        Email = x.Email,
                        Role = x.Role,
                        Password = x.Password
                    }
                )
                .SingleOrDefaultAsync(cancellationToken);
            
            if (user is null)
                throw new ValidationErrorsException(nameof(request.Email), "User not found by given email.","authenticate_user_email_not_found");
            
            if (!_paswordHasher.VerifyHashedPassword(user.Password, request.Password))
                throw new Exception("Invalid credentials");

            var jwt = user switch
            {
                { Role: "User" } => _jwtTokenGenerator.GenerateJwt(user.Email, [UserRole.User]),
                { Role: "Analyst" } => _jwtTokenGenerator.GenerateJwt(user.Email, [UserRole.Analyst]),
                { Role: "Administrator" } => _jwtTokenGenerator.GenerateJwt(user.Email, [
                    UserRole.Administrator, UserRole.User
                ]),
                _ => throw new Exception("Invalid user role in DB.")
            };

            var refreshToken = await _jwtTokenGenerator.GenerateRefreshToken(user.Id, cancellationToken);

            return new Response(jwt, refreshToken.Token);
        }
    }
}