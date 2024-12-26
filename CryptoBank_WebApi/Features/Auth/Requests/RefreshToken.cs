using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using FastEndpoints;
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
    public class Endpoint(IMediator mediator, IHttpContextAccessor httpContextAccessor, 
        IOptions<AuthConfigurations> authConfigs) : EndpointWithoutRequest<EndpointResponse>
    {
        public override async Task<EndpointResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var refreshToken = httpContextAccessor.HttpContext!.Request.Cookies["RefreshToken"];
            var principal = httpContextAccessor.HttpContext.User;

            if (refreshToken is null)
                throw new Exception("Invalid refresh token");

            var request = new Request(refreshToken!, principal);
            var response = await mediator.Send(request, cancellationToken);

            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.Add(authConfigs.Value.Jwt.RefreshTokenExpiration)
            };
            httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", response.refreshToken, cookie);

            return new EndpointResponse(response.jwt);
        }
    }

    public record Request(string RefreshToken, ClaimsPrincipal Principal) : IRequest<Response>;
    public record Response(string jwt, string refreshToken);
    public record EndpointResponse(string jwt);

    public class RequestHandler(CryptoBank_DbContext dbContext, TokenGenerator jwtTokenGenerator) 
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var dbToken = await dbContext.UserRefreshTokens
                .Where(x => x.Token == request.RefreshToken)
                .SingleOrDefaultAsync(cancellationToken);
            if (dbToken is null)
                throw new Exception("Invalid refresh token");
            
            if (!request.Principal.HasClaim(x => x.Type == ClaimTypes.Email) ||
                !request.Principal.HasClaim(x => x.Type == ClaimTypes.Role))
                throw new Exception("Invalid auth token");

            var claims = request.Principal.Claims.ToList();
            var email = claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var roles = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => Enum.Parse<UserRole>(x.Value))
                .ToArray();
            
            var newToken = jwtTokenGenerator.GenerateJwt(email!, roles);
            var newRefreshToken = await jwtTokenGenerator.GenerateRefreshToken(dbToken.UserId, cancellationToken);

            return new Response(newToken, newRefreshToken.Token);
        }
    }
}