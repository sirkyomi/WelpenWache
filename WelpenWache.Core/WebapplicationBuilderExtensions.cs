using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WelpenWache.Core.Database;
using WelpenWache.Core.Features.Intern.DependencyInjection;
using WelpenWache.Core.Services;

namespace WelpenWache.Core;

public static class WebapplicationBuilderExtensions {
    public static IServiceCollection AddWelpenWacheCoreServices(this IServiceCollection services, string connectionString) {
        services.AddDbContextFactory<WelpenWacheContext>(
            x => x.UseSqlServer(connectionString));
        services.AddInternServices();
        services.AddScoped<PermissionService>();
        services.AddScoped<AccessRequestService>();
        services.AddSingleton<SetupService>();
        //other Services
        
        return services;
    }

}