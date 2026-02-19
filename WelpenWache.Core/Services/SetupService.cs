using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Database.Models;

namespace WelpenWache.Core.Services;

public class SetupService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;
    
    public SetupService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<bool> IsSetupRequiredAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return !await context.UserPermissions.AnyAsync();
    }

    public async Task CreateAdminUserAsync(string windowsSid, string username) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Check if users already exist
        if (await context.UserPermissions.AnyAsync()) {
            throw new InvalidOperationException("Setup has already been completed.");
        }

        // Add all permissions
        var allPermissions = Enum.GetValues<Permissions>();
        foreach (var permission in allPermissions) {
            context.UserPermissions.Add(new UserPermission {
                Sid = windowsSid,
                Permission = permission
            });
        }

        if (!string.IsNullOrWhiteSpace(username)) {
            context.AccessRequests.Add(new AccessRequest {
                Sid = windowsSid,
                Username = username,
                RequestedAt = DateTime.UtcNow,
                Status = AccessRequestStatus.Approved,
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = username
            });
        }

        await context.SaveChangesAsync();
    }
}

