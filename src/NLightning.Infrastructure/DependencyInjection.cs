using Microsoft.Extensions.DependencyInjection;
using NLightning.Application.Node.Interfaces;
using NLightning.Domain.Crypto.Hashes;
using NLightning.Domain.Protocol.Factories;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Infrastructure.Crypto.Hashes;
using NLightning.Infrastructure.Node.Factories;
using NLightning.Infrastructure.Node.Interfaces;
using NLightning.Infrastructure.Node.Managers;
using NLightning.Infrastructure.Protocol.Factories;
using NLightning.Infrastructure.Transport.Factories;
using NLightning.Infrastructure.Transport.Services;
using NLightning.Node.Interfaces;

namespace NLightning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Singleton services (one instance throughout the application)
        services.AddSingleton<IChannelIdFactory, ChannelIdFactory>();
        services.AddSingleton<IMessageServiceFactory, MessageServiceFactory>();
        services.AddSingleton<IPeerManager, PeerManager>();
        services.AddSingleton<IPeerServiceFactory, PeerServiceFactory>();
        services.AddSingleton<IPingPongServiceFactory, PingPongServiceFactory>();
        services.AddSingleton<ITcpListenerService, TcpListenerService>();
        services.AddSingleton<ISha256, Sha256>();
        services.AddSingleton<ITransportServiceFactory, TransportServiceFactory>();

        return services;
    }
}