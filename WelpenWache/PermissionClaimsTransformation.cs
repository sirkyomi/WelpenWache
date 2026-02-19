using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WelpenWache.Core;
using WelpenWache.Core.Database;

namespace WelpenWache;

public class PermissionClaimsTransformation : IClaimsTransformation {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;

    public PermissionClaimsTransformation(IDbContextFactory<WelpenWacheContext> contextFactory) {
        _contextFactory = contextFactory;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
        // If user is not authenticated, return as-is
        if (principal.Identity is not { IsAuthenticated: true }) {
            return principal;
        }

        var sid = principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
        if (string.IsNullOrEmpty(sid)) {
            return principal;
        }

        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var dbPermissions = await dbContext.UserPermissions
            .Where(p => p.Sid == sid)
            .ToListAsync();

        // Remove existing permission claims so changes are reflected immediately
        foreach (var identity in principal.Identities) {
            var toRemove = identity.FindAll(nameof(Permissions)).ToList();
            foreach (var claim in toRemove) {
                identity.RemoveClaim(claim);
            }
        }

        if (dbPermissions.Any()) {
            var identity = new ClaimsIdentity();
            identity.AddClaims(dbPermissions.Select(p => new Claim(nameof(Permissions), p.Permission.ToString())));
            principal.AddIdentity(identity);
        }

        return principal;
    }
}

