using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;

namespace WelpenWache.Core.Services;

public class PermissionService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;
    
    public PermissionService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<Permissions>> GetPermissionsForUserAsync(string windowsSid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserPermissions
            .Where(p => p.Sid == windowsSid)
            .Select(p => p.Permission)
            .ToListAsync();
    }
}