using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Features.Intern.Models;

namespace WelpenWache.Core.Features.Intern;

public class InternService : IInternService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;

    public InternService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<Guid> CreateInternAsync(InternCreateRequest request) {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var startDate = request.StartDate.Date;
        var endDate = request.EndDate.Date;
        ValidateInternRange(startDate, endDate);
        ValidateInternName(request.Name, request.Surname);
        var assignmentsByDate = ValidateAndNormalizeAssignments(request.DayAssignments, startDate, endDate);
        await ValidateTeamIdsAsync(context, assignmentsByDate.Values.Distinct().ToList());

        var intern = new Database.Models.Intern {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            TeamAssignments = assignmentsByDate
                .Select(x => new Database.Models.InternTeamAssignment {
                    Id = Guid.NewGuid(),
                    Date = x.Key,
                    TeamId = x.Value
                })
                .ToList()
        };

        await context.Interns.AddAsync(intern);
        await context.SaveChangesAsync();
        return intern.Id;
    }

    public async Task<List<InternDto>> GetInternsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var interns = await context.Interns
            .AsNoTracking()
            .Include(x => x.TeamAssignments)
            .ThenInclude(x => x.Team)
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.Surname)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return interns.Select(MapIntern).ToList();
    }

    public async Task<InternDto> GetInternAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var internDb = await context.Interns
            .AsNoTracking()
            .Include(x => x.TeamAssignments)
            .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (internDb == null) {
            throw new NullReferenceException();
        }

        return MapIntern(internDb);
    }

    public async Task UpdateInternAsync(InternDto dto) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var internDb = await context.Interns.FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (internDb == null) {
            return;
        }

        var startDate = dto.StartDate.Date;
        var endDate = dto.EndDate.Date;
        ValidateInternRange(startDate, endDate);
        ValidateInternName(dto.Name, dto.Surname);
        var assignmentRequests = dto.DayAssignments
            .Select(x => new InternDayAssignmentCreateRequest(x.Date, x.TeamId))
            .ToList();
        var assignmentsByDate = ValidateAndNormalizeAssignments(assignmentRequests, startDate, endDate);
        await ValidateTeamIdsAsync(context, assignmentsByDate.Values.Distinct().ToList());

        internDb.Name = dto.Name.Trim();
        internDb.Surname = dto.Surname.Trim();
        internDb.StartDate = startDate;
        internDb.EndDate = endDate;

        await context.InternTeamAssignments
            .Where(x => x.InternId == internDb.Id)
            .ExecuteDeleteAsync();

        var newAssignments = assignmentsByDate
            .Select(x => new Database.Models.InternTeamAssignment {
                Id = Guid.NewGuid(),
                Date = x.Key,
                TeamId = x.Value,
                InternId = internDb.Id
            })
            .ToList();

        await context.InternTeamAssignments.AddRangeAsync(newAssignments);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task DeleteInternAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var intern = await context.Interns.FindAsync(id);
        if (intern == null) {
            return;
        }

        context.Interns.Remove(intern);
        await context.SaveChangesAsync();
    }

    private static InternDto MapIntern(Database.Models.Intern x) {
        return new InternDto {
            Id = x.Id,
            Name = x.Name,
            Surname = x.Surname,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            DayAssignments = x.TeamAssignments
                .OrderBy(a => a.Date)
                .Select(a => new InternDayAssignmentDto {
                    Date = a.Date,
                    TeamId = a.TeamId,
                    TeamName = a.Team.Name
                })
                .ToList()
        };
    }

    private static Dictionary<DateTime, Guid> ValidateAndNormalizeAssignments(
        IReadOnlyCollection<InternDayAssignmentCreateRequest>? assignments,
        DateTime startDate,
        DateTime endDate) {
        if (assignments == null || assignments.Count == 0) {
            throw new InvalidOperationException("Für jeden Praktikumstag muss ein Team zugeordnet sein.");
        }

        var normalized = new Dictionary<DateTime, Guid>();
        foreach (var assignment in assignments) {
            var date = assignment.Date.Date;
            if (date < startDate || date > endDate) {
                throw new InvalidOperationException("Teamzuordnung außerhalb des Praktikumszeitraums gefunden.");
            }

            if (assignment.TeamId == Guid.Empty) {
                throw new InvalidOperationException("Ungültige Teamzuordnung gefunden.");
            }

            if (!normalized.TryAdd(date, assignment.TeamId)) {
                throw new InvalidOperationException($"Für den Tag {date:dd.MM.yyyy} existieren mehrere Teamzuordnungen.");
            }
        }

        foreach (var date in GetAllDays(startDate, endDate)) {
            if (!normalized.ContainsKey(date)) {
                throw new InvalidOperationException($"Für den Tag {date:dd.MM.yyyy} fehlt die Teamzuordnung.");
            }
        }

        return normalized;
    }

    private static IEnumerable<DateTime> GetAllDays(DateTime startDate, DateTime endDate) {
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1)) {
            yield return date;
        }
    }

    private static void ValidateInternRange(DateTime startDate, DateTime endDate) {
        if (startDate > endDate) {
            throw new InvalidOperationException("Das Startdatum darf nicht nach dem Enddatum liegen.");
        }
    }

    private static void ValidateInternName(string name, string surname) {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname)) {
            throw new InvalidOperationException("Vorname und Nachname sind erforderlich.");
        }
    }

    private static async Task ValidateTeamIdsAsync(WelpenWacheContext context, IReadOnlyCollection<Guid> teamIds) {
        var teamIdList = teamIds.Distinct().ToList();
        var existingCount = await context.Teams.CountAsync(x => teamIdList.Contains(x.Id));
        if (existingCount != teamIdList.Count) {
            throw new InvalidOperationException("Mindestens ein ausgewähltes Team existiert nicht mehr.");
        }
    }
}


