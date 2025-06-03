using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLightning.Application.Channels.Managers;
using NLightning.Application.Protocol.Factories;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Application;

using Node.Services;
using Domain.Protocol.Services;

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
        services.AddSingleton<IPingPongService, PingPongService>();
        services.AddSingleton<IMessageFactory, MessageFactory>();
        services.AddSingleton<IChannelManager>(sp =>
        {
            var channelFactory = sp.GetRequiredService<IChannelFactory>();
            var channelIdFactory = sp.GetRequiredService<IChannelIdFactory>();
            var messageFactory = sp.GetRequiredService<IMessageFactory>();
            var nodeOptions = sp.GetRequiredService<IOptions<NodeOptions>>().Value;
            var lightningSigner = sp.GetRequiredService<ILightningSigner>();
            return new ChannelManager(channelFactory, channelIdFactory, messageFactory, nodeOptions, 
                lightningSigner);
        });
        
        // Add other application services here
        
        return services;
    }
}
