using System.Security.Claims;

namespace CryptoBank_WebApi.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetClaim(this ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.Claims.SingleOrDefault(x => x.Type == claimType);
        return claim?.Value ?? string.Empty;
    }
}