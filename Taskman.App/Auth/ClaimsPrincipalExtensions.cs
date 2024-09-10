using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication;

namespace Taskman.Controllers;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal claims)
    {
        Claim? userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        return (userIdClaim != null) ? Guid.Parse(userIdClaim.Value) : null;
    }
}