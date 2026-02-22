using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Features.Team.Models;

namespace WelpenWache.Core.Features.Team;

public class TeamService : ITeamService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;

    public TeamService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<TeamDto>> GetTeamsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TeamDto {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    public async Task<TeamDto> GetTeamAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var team = await context.Teams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (team == null) {
            throw new NullReferenceException("Team not found.");
        }

        return new TeamDto {
            Id = team.Id,
            Name = team.Name
        };
    }

    public async Task<Guid> CreateTeamAsync(string name) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cleanedName = name.Trim();
        if (string.IsNullOrWhiteSpace(cleanedName)) {
            throw new InvalidOperationException("Teamname darf nicht leer sein.");
        }

        var exists = await context.Teams.AnyAsync(x => x.Name == cleanedName);
        if (exists) {
            throw new InvalidOperationException("Ein Team mit diesem Namen existiert bereits.");
        }

        var team = new Database.Models.Team {
            Id = Guid.NewGuid(),
            Name = cleanedName
        };

        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        return team.Id;
    }

    public async Task UpdateTeamAsync(TeamDto dto) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var team = await context.Teams.FindAsync(dto.Id);
        if (team == null) {
            return;
        }

        var cleanedName = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(cleanedName)) {
            throw new InvalidOperationException("Teamname darf nicht leer sein.");
        }

        var exists = await context.Teams.AnyAsync(x => x.Id != dto.Id && x.Name == cleanedName);
        if (exists) {
            throw new InvalidOperationException("Ein Team mit diesem Namen existiert bereits.");
        }

        team.Name = cleanedName;
        await context.SaveChangesAsync();
    }

    public async Task DeleteTeamAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var team = await context.Teams.FindAsync(id);
        if (team == null) {
            return;
        }

        var today = DateTime.Today;
        var hasCurrentOrFutureAssignments = await context.InternTeamAssignments
            .AnyAsync(x => x.TeamId == id && x.Intern.EndDate.Date >= today);

        if (hasCurrentOrFutureAssignments) {
            throw new InvalidOperationException(
                "Team kann nicht gelöscht werden, solange aktuelle oder zukünftige Praktikantenzuordnungen existieren.");
        }

        var historicalAssignments = await context.InternTeamAssignments
            .Where(x => x.TeamId == id)
            .ToListAsync();

        if (historicalAssignments.Count > 0) {
            context.InternTeamAssignments.RemoveRange(historicalAssignments);
        }

        context.Teams.Remove(team);
        await context.SaveChangesAsync();
    }
}
