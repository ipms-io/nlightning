using Microsoft.Extensions.DependencyInjection;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Infrastructure.Repositories.Memory;

namespace NLightning.Infrastructure.Repositories;

using Domain.Persistence.Interfaces;

/// <summary>
/// Extension methods for setting up Persistence infrastructure services in an IServiceCollection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Bitcoin infrastructure services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRepositoriesInfrastructureServices(this IServiceCollection services)
    {
        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register memory repositories
        services.AddSingleton<IChannelMemoryRepository, ChannelMemoryRepository>();

        return services;
    }
}