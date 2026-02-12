using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Database.Models;

namespace WelpenWache.Core.Services;

public class AccessRequestService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;
    
    public AccessRequestService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<bool> HasPendingOrApprovedRequestAsync(string sid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AccessRequests
            .AnyAsync(r => r.Sid == sid && (r.Status == AccessRequestStatus.Pending || r.Status == AccessRequestStatus.Approved));
    }

    public async Task<AccessRequest?> GetActiveRequestAsync(string sid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AccessRequests
            .Where(r => r.Sid == sid)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAccessRequestAsync(string sid, string username) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingRequest = await context.AccessRequests
            .FirstOrDefaultAsync(r => r.Sid == sid && r.Status == AccessRequestStatus.Pending);
        
        if (existingRequest != null) {
            return; // Already has a pending request
        }

        var request = new AccessRequest {
            Sid = sid,
            Username = username
        };

        context.AccessRequests.Add(request);
        await context.SaveChangesAsync();
    }

    public async Task<List<AccessRequest>> GetPendingRequestsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AccessRequests
            .Where(r => r.Status == AccessRequestStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync();
    }

    public async Task<List<AccessRequest>> GetAllRequestsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AccessRequests
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();
    }

    public async Task ApproveRequestAsync(int requestId, string processedBy) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var request = await context.AccessRequests.FindAsync(requestId);
        
        if (request == null || request.Status != AccessRequestStatus.Pending) {
            return;
        }

        request.Status = AccessRequestStatus.Approved;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = processedBy;

        await context.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(int requestId, string processedBy) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var request = await context.AccessRequests.FindAsync(requestId);
        
        if (request == null || request.Status != AccessRequestStatus.Pending) {
            return;
        }

        request.Status = AccessRequestStatus.Rejected;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = processedBy;

        await context.SaveChangesAsync();
    }
}

