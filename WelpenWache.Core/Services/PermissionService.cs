using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Models;

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

    public async Task<List<UserWithPermissions>> GetUsersWithPermissionsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var userPermissions = await context.UserPermissions
            .AsNoTracking()
            .ToListAsync();

        var usernames = await context.AccessRequests
            .AsNoTracking()
            .GroupBy(r => r.Sid)
            .Select(g => new {
                Sid = g.Key,
                Username = g.OrderByDescending(r => r.RequestedAt)
                    .Select(r => r.Username)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var usernameMap = usernames
            .Where(u => !string.IsNullOrWhiteSpace(u.Username))
            .ToDictionary(u => u.Sid, u => u.Username!);

        return userPermissions
            .GroupBy(p => p.Sid)
            .Select(group => new UserWithPermissions {
                Sid = group.Key,
                Username = usernameMap.TryGetValue(group.Key, out var username)
                    ? username
                    : group.Key,
                Permissions = group.Select(p => p.Permission).OrderBy(p => p.ToString()).ToList()
            })
            .OrderBy(u => u.Username)
            .ToList();
    }

    public async Task<bool> HasAnyPermissionAsync(string windowsSid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserPermissions.AnyAsync(p => p.Sid == windowsSid);
    }

    public async Task AddPermissionAsync(string sid, Permissions permission) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var exists = await context.UserPermissions
            .AnyAsync(p => p.Sid == sid && p.Permission == permission);
        
        if (exists) {
            return;
        }

        context.UserPermissions.Add(new Database.Models.UserPermission {
            Sid = sid,
            Permission = permission
        });

        await context.SaveChangesAsync();
    }

    public async Task SetPermissionsAsync(string sid, IEnumerable<Permissions> permissions) {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var desired = permissions.Distinct().ToHashSet();
        var existing = await context.UserPermissions
            .Where(p => p.Sid == sid)
            .ToListAsync();

        var toRemove = existing
            .Where(p => !desired.Contains(p.Permission))
            .ToList();

        if (toRemove.Any()) {
            context.UserPermissions.RemoveRange(toRemove);
        }

        var existingSet = existing.Select(p => p.Permission).ToHashSet();
        var toAdd = desired
            .Where(p => !existingSet.Contains(p))
            .Select(p => new Database.Models.UserPermission {
                Sid = sid,
                Permission = p
            })
            .ToList();

        if (toAdd.Any()) {
            context.UserPermissions.AddRange(toAdd);
        }

        if (toRemove.Any() || toAdd.Any()) {
            await context.SaveChangesAsync();
        }
    }

    public async Task RemovePermissionAsync(string sid, Permissions permission) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var userPermission = await context.UserPermissions
            .FirstOrDefaultAsync(p => p.Sid == sid && p.Permission == permission);
        
        if (userPermission != null) {
            context.UserPermissions.Remove(userPermission);
            await context.SaveChangesAsync();
        }
    }
}
