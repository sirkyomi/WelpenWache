using WelpenWache.Core.Features.Team.Models;

namespace WelpenWache.Core.Features.Team;

public interface ITeamService {
    Task<List<TeamDto>> GetTeamsAsync();
    Task<TeamDto> GetTeamAsync(Guid id);
    Task<Guid> CreateTeamAsync(string name);
    Task UpdateTeamAsync(TeamDto dto);
    Task DeleteTeamAsync(Guid id);
}
