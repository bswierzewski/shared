using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Modules;

namespace Shared.Infrastructure.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration, params IModule[] modules)
    {
        foreach (var module in modules)
        {
            module.Register(services, configuration);
            services.AddSingleton(module);
        }

        return services;
    }

    public static IApplicationBuilder UseModules(this IApplicationBuilder app, IConfiguration configuration)
    {
        var modules = app.ApplicationServices.GetServices<IModule>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<IModule>>();

        foreach (var module in modules)
        {
            try
            {
                logger.LogInformation("Configuring module '{ModuleName}'...", module.Name);
                module.Use(app, configuration);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error configuring module '{ModuleName}'", module.Name);
                throw;
            }
        }

        return app;
    }

    public static async Task InitModules(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var modules = serviceProvider.GetServices<IModule>();
        var logger = serviceProvider.GetRequiredService<ILogger<IModule>>();

        foreach (var module in modules)
        {
            try
            {
                logger.LogInformation("Initializing module '{ModuleName}'...", module.Name);
                await module.Initialize(serviceProvider, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing module '{ModuleName}'", module.Name);
                throw;
            }
        }
    }
}
