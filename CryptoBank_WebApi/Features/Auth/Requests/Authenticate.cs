using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Common;
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
    public class Endpoint(IMediator mediator) : Endpoint<Request, Response>
    {
        public override async Task<Response> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await mediator.Send(request, cancellationToken);
    }

    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(AuthToken Token);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CryptoBank_DbContext _dbContext;
        private readonly AuthConfigurations _authConfigs;
        private readonly Argon2IdPasswordHasher _paswordHasher;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public RequestHandler(CryptoBank_DbContext dbContext, IOptions<AuthConfigurations> authConfigs,
            Argon2IdPasswordHasher paswordHasher, JwtTokenGenerator jwtTokenGenerator)
        {
            _dbContext = dbContext;
            _authConfigs = authConfigs.Value;
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
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                throw new Exception("Invalid credentials");

            if (!_paswordHasher.VerifyHashedPassword(user.Password, request.Password))
                throw new Exception("Invalid credentials");

            var jwt = user switch
            {
                { Role: "User" } => _jwtTokenGenerator.GenerateJwt(user.Email, new[] { UserRole.User }),
                { Role: "Analyst" } => _jwtTokenGenerator.GenerateJwt(user.Email, new[] { UserRole.Analyst }),
                { Role: "Administrator" } => _jwtTokenGenerator.GenerateJwt(user.Email, new[] { UserRole.Administrator, UserRole.User }),
                _ => throw new Exception("Invalid user role in DB.")
            };
            
            var refreshToken = await _jwtTokenGenerator.GenerateRefreshToken(user.Id, cancellationToken);

            return new Response(new AuthToken { AccessToken = jwt, RefreshToken = refreshToken.Token });
        }
    }
}