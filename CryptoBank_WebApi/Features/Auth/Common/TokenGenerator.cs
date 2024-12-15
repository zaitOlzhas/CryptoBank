using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank_WebApi.Features.Auth.Common;

public class TokenGenerator
{
    private readonly AuthConfigurations _authConfigs;
    private readonly CryptoBank_DbContext _dbContext;
    public TokenGenerator(IOptions<AuthConfigurations> authConfigs, CryptoBank_DbContext dbContext)
    {
        _authConfigs = authConfigs.Value;
        _dbContext = dbContext;
    }

    public async Task<UserRefreshToken> GenerateRefreshToken(int userId, CancellationToken cancellationToken)
    {
        var refreshToken = "";
        using var rng = RandomNumberGenerator.Create();

        var randomNumber = new byte[32];
        rng.GetBytes(randomNumber);
        refreshToken = Convert.ToBase64String(randomNumber);

        var refreshTokenRecord = new UserRefreshToken
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow + _authConfigs.Jwt.RefreshTokenExpiration,
            UserId = userId
        };

        var oldTokens = await _dbContext.UserRefreshTokens.Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        _dbContext.UserRefreshTokens.RemoveRange(oldTokens);

        await _dbContext.UserRefreshTokens.AddAsync(refreshTokenRecord, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return refreshTokenRecord;
    }

    public string GenerateJwt(string email, UserRole[] roles)
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
    
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _authConfigs.Jwt.Issuer,
            ValidAudience = _authConfigs.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfigs.Jwt.SigningKey))
                    
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