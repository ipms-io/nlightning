using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLightning.Application;

using Channels.Handlers.Interfaces;
using Channels.Managers;
using Domain.Bitcoin.Interfaces;
using Domain.Channels.Interfaces;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Services;
using Node.Services;
using Protocol.Factories;

/// <summary>
/// Extension methods for setting up application services in an IServiceCollection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Singleton services (one instance throughout the application)
        services.AddSingleton<IChannelManager>(sp =>
        {
            var channelMemoryRepository = sp.GetRequiredService<IChannelMemoryRepository>();
            var lightningSigner = sp.GetRequiredService<ILightningSigner>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new ChannelManager(channelMemoryRepository, loggerFactory.CreateLogger<ChannelManager>(),
                                      lightningSigner, sp);
        });
        services.AddSingleton<IMessageFactory, MessageFactory>();
        services.AddSingleton<IPingPongService, PingPongService>();

        // Automatically register all channel message handlers
        services.AddChannelMessageHandlers();

        return services;
    }

    /// <summary>
    /// Registers all classes that implement IChannelMessageHandler&lt;T&gt; from the current assembly
    /// </summary>
    private static void AddChannelMessageHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Find all types that implement IChannelMessageHandler<>
        var handlerTypes = assembly
                          .GetTypes()
                          .Where(type => type is { IsClass: true, IsAbstract: false })
                          .Where(type => type.GetInterfaces()
                                             .Any(i => i.IsGenericType
                                                    && i.GetGenericTypeDefinition() ==
                                                       typeof(IChannelMessageHandler<>)))
                          .ToArray();

        foreach (var handlerType in handlerTypes)
        {
            // Get the interface this handler implements
            var handlerInterface = handlerType
                                  .GetInterfaces()
                                  .First(i => i.IsGenericType
                                           && i.GetGenericTypeDefinition() == typeof(IChannelMessageHandler<>));

            // Register as scoped (or singleton if you prefer)
            services.AddScoped(handlerInterface, handlerType);
            // services.AddScoped(handlerType); // Also register the concrete type for direct resolution
        }
    }
}