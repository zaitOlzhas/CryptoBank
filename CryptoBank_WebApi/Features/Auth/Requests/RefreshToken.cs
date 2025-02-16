using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Exceptions;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank_WebApi.Features.Auth.Requests;

public class RefreshToken
{
    [HttpGet("/refresh-token")]
    [AllowAnonymous]
    public class Endpoint(
        IMediator mediator,
        IOptions<AuthConfigurations> authConfigs) : EndpointWithoutRequest<EndpointResponse>
    {
        public override async Task<EndpointResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            this.HttpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken);
            var principal = this.HttpContext.User;

            if (refreshToken is null)
                throw new ValidationErrorsException("RefreshToken", "Missing refresh token","refresh_token_validation_refresh_token_empty");

            if (!principal.HasClaim(x => x.Type == ClaimTypes.Email))
                throw new ValidationErrorsException("Email", "Missing email from auth token","refresh_token_validation_email_empty");
            
            var email = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var roles = principal.Claims.Where(x => x.Type == ClaimTypes.Role)
                .Select(x => Enum.Parse<UserRole>(x.Value))
                .ToArray();
            var request = new Request(refreshToken, email, roles);
            var response = await mediator.Send(request, cancellationToken);

            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.Add(authConfigs.Value.Jwt.RefreshTokenExpiration)
            };
            this.HttpContext.Response.Cookies.Append("RefreshToken", response.RefreshToken, cookie);

            return new EndpointResponse(response.Jwt);
        }
    }

    public record Request(string RefreshToken, string? Email, UserRole[] Roles) : IRequest<Response>;

    public record Response(string Jwt, string RefreshToken);

    public record EndpointResponse(string Jwt);

    public class RequestValidator : AbstractValidator<Request>
    {
        private const string MessagePrefix = "refresh_token_validation_";

        public RequestValidator()
        {
            RuleFor(x => x.Email)
                .ValidateEmail(MessagePrefix);
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(MessagePrefix + "refresh_token_empty");
            RuleFor(x => x.Roles)
                .NotEmpty().WithMessage(MessagePrefix + "roles_empty");
        }
    }

    public class RequestHandler(CryptoBank_DbContext dbContext, TokenGenerator jwtTokenGenerator)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            ////TODO: Нужно ли проверить токен на наличие в бд в валидаторе?
            var dbToken = await dbContext.UserRefreshTokens
                .Where(x => x.Token == request.RefreshToken)
                .SingleOrDefaultAsync(cancellationToken);
            if (dbToken is null)
                throw new ValidationErrorsException(nameof(request.RefreshToken), "Refresh token not found.","refresh_token_validation_token_not_found");

            var user = await dbContext.Users
                .Where(x => x.Email == request.Email!.ToLower())
                .SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                throw new ValidationErrorsException(nameof(request.Email), "User not found by given email.","refresh_token_validation_user_not_found");

            var newToken = jwtTokenGenerator.GenerateJwt(request.Email!, request.Roles);
            var newRefreshToken = await jwtTokenGenerator.GenerateRefreshToken(dbToken.UserId, cancellationToken);

            return new Response(newToken, newRefreshToken.Token);
        }
    }
}