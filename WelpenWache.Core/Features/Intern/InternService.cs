using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;
using WelpenWache.Core.Features.Intern.Models;

namespace WelpenWache.Core.Features.Intern;

public class InternService : IInternService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;
    
    public InternService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;
    
    public async Task<Guid> CreateInternAsync(InternCreateRequest request) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var intern = new Database.Models.Intern() {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Surname = request.Surname,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await context.Interns.AddAsync(intern);
        await context.SaveChangesAsync();
        return intern.Id;
    }

    public async Task<List<InternDto>> GetInternsAsync() {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Interns.AsNoTracking().Select(x => new InternDto {
                Id = x.Id,
                Name = x.Name,
                Surname = x.Surname,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .ToListAsync();
    }

    public async Task<InternDto> GetInternAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var internDb = await context.Interns.FindAsync(id);
        if (internDb == null)
            throw new NullReferenceException();
        return new InternDto {
            Id = internDb.Id,
            Name = internDb.Name,
            Surname = internDb.Surname,
            StartDate = internDb.StartDate,
            EndDate = internDb.EndDate
        };

    }

    public async Task UpdateInternAsync(InternDto dto) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var internDb = await context.Interns.FindAsync(dto.Id);
        if (internDb == null)
            return;

        internDb.Name = dto.Name;
        internDb.Surname = dto.Surname;
        internDb.StartDate = dto.StartDate;
        internDb.EndDate = dto.EndDate;
        await context.SaveChangesAsync();
    }

    public async Task DeleteInternAsync(Guid id) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var intern = await context.Interns.FindAsync(id);
        if (intern == null)
            return;
        context.Interns.Remove(intern);
        await context.SaveChangesAsync();
    }
}