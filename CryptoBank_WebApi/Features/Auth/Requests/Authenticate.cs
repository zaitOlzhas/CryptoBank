using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.Auth.Model;
using FastEndpoints;
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
    public class Endpoint(IMediator mediator) : Endpoint<Request,Response>
    {
        public override async Task<Response> ExecuteAsync(Request request,CancellationToken cancellationToken) =>
            await mediator.Send(request,cancellationToken);
    }

    public record Request(string Email, string Password) : IRequest<Response>;
    public record Response(string Jwt);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CryptoBank_DbContext _dbContext;
        private readonly AuthConfigurations _authConfigs;

        public RequestHandler(CryptoBank_DbContext dbContext,IOptions<AuthConfigurations> authConfigs)
        {
            _dbContext = dbContext;
            _authConfigs = authConfigs.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Where(x => x.Email == request.Email && x.Password == request.Password)
                .Select(x => new UserModel
                    {
                        Id = x.Id,
                        Email = x.Email,
                        Password = x.Password,
                        Role = x.Role,
                    }
                )
                .FirstOrDefaultAsync(cancellationToken);
           
           var jwt  = user switch
           {
               {Role: "User"} => GenerateJwt(user.Email, new [] {UserRole.User}),
               {Role: "Analyst"} => GenerateJwt(user.Email, new [] {UserRole.Analyst}),
               {Role: "Administrator"} => GenerateJwt(user.Email, new [] {UserRole.Administrator}),
               _ => throw new Exception("Invalid user role in DB.")
           };
           return await Task.FromResult(new Response(jwt));
        }
        private string GenerateJwt(string email, UserRole[] roles, int? rank = null)
        {
            var claims = new List<Claim>
            {
                new("email", email),
            };

            foreach (var role in roles)
            {
                claims.Add(new("role", role.ToString()));
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfigs.Jwt.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now + _authConfigs.Jwt.Expiration;

            var token = new JwtSecurityToken(
                _authConfigs.Jwt.Issuer,
                _authConfigs.Jwt.Audience,
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}