using Microsoft.Extensions.DependencyInjection;

namespace WelpenWache.Core.Features.Intern.DependencyInjection;

public static class InternDependencyInjection {
    public static IServiceCollection AddInternServices(this IServiceCollection services) {
        services.AddScoped<IInternService, InternService>();
        return services;
    }
}