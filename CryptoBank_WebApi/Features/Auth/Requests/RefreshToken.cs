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
    [HttpPost("/refresh-token")]
    [AllowAnonymous]
    public class Endpoint(IMediator mediator) : Endpoint<Request,Response>
    {
        public override async Task<Response> ExecuteAsync(Request request,CancellationToken cancellationToken) =>
            await mediator.Send(request,cancellationToken);
    }

    public record Request(string jwt) : IRequest<Response>;
    public record Response(string jwt);

    public class RequestHandler(CryptoBank_DbContext dbContext, JwtTokenGenerator jwtTokenGenerator, IHttpContextAccessor httpContextAccessor, IOptions<AuthConfigurations> authConfigs) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var refreshToken = httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];
            var dbToken = await dbContext.UserRefreshTokens
                .Where(x => x.Token == refreshToken)
                .FirstOrDefaultAsync(cancellationToken);
            if (dbToken is null)
                throw new Exception("Invalid refresh token");

            var principal = jwtTokenGenerator.GetPrincipalFromExpiredToken(request.jwt);

            if (!principal.HasClaim(x => x.Type == ClaimTypes.Email) ||
                !principal.HasClaim(x => x.Type == ClaimTypes.Role))
                throw new Exception("Invalid auth token");

            var claims = principal.Claims.ToList();
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var roles = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => Enum.Parse<UserRole>(x.Value))
                .ToArray();

            var newToken = jwtTokenGenerator.GenerateJwt(email, roles);

            var newRefreshToken = await jwtTokenGenerator.GenerateRefreshToken(dbToken.UserId, cancellationToken);

            httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.Add(authConfigs.Value.Jwt.RefreshTokenExpiration)
            });
            
            return new Response(newToken);
        }
    }
}