using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebExplain.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("The authenticated user has no subject claim.");
        return Guid.Parse(value);
    }
}
