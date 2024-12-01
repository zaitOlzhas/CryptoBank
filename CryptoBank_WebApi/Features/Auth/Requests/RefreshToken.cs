using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
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

    public record Request(AuthToken token) : IRequest<Response>;
    public record Response(AuthToken token);

    public class RequestHandler(CryptoBank_DbContext dbContext, IOptions<AuthConfigurations> authConfigurations) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var dbToken = await dbContext.UserRefreshTokens
                .Where(x => x.Token == request.token.RefreshToken)
                .FirstOrDefaultAsync(cancellationToken);
            if (dbToken is null)
                throw new Exception("Invalid token");
            var principal = GetPrincipalFromExpiredToken(request.token.AccessToken);
            var claims = principal.Claims;
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var roles = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => Enum.Parse<UserRole>(x.Value))
                .ToArray();
            var newToken = GenerateJwt(email, roles);

            var newRefreshToken = await GenerateRefreshToken(dbToken.UserId, cancellationToken);

            return new Response(new AuthToken
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        private async Task<UserRefreshToken> GenerateRefreshToken(int userId, CancellationToken cancellationToken)
        {
            var refreshToken = new UserRefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow + authConfigurations.Value.Jwt.RefreshTokenExpiration,
                UserId = userId
            };
            
            var oldTokens = await dbContext.UserRefreshTokens.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
            dbContext.UserRefreshTokens.RemoveRange(oldTokens);
            
            await dbContext.UserRefreshTokens.AddAsync(refreshToken, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }
        private string GenerateJwt(string email, UserRole[] roles)
            {
                //TODO: User custom claims
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Email, email),
                };
        
                foreach (var role in roles)
                {
                    claims.Add(new(ClaimTypes.Role, role.ToString()));
                }
        
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfigurations.Value.Jwt.SigningKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now + authConfigurations.Value.Jwt.Expiration;
        
                var token = new JwtSecurityToken(
                    authConfigurations.Value.Jwt.Issuer,
                    authConfigurations.Value.Jwt.Audience,
                    claims,
                    expires: expires,
                    signingCredentials: credentials
                );
        
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authConfigurations.Value.Jwt.Issuer,
                    ValidAudience = authConfigurations.Value.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfigurations.Value.Jwt.SigningKey))
                    
                };
        
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }
        
                return principal;
            }
    }
    
}