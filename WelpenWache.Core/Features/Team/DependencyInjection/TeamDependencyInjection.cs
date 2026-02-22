using Microsoft.Extensions.DependencyInjection;

namespace WelpenWache.Core.Features.Team.DependencyInjection;

public static class TeamDependencyInjection {
    public static IServiceCollection AddTeamServices(this IServiceCollection services) {
        services.AddScoped<ITeamService, TeamService>();
        return services;
    }
}
