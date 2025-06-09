using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Infrastructure;

using Application.Node.Interfaces;
using Crypto.Hashes;
using Domain.Crypto.Hashes;
using Domain.Protocol.Interfaces;
using Node.Factories;
using Node.Interfaces;
using Node.Managers;
using Protocol.Factories;
using Transport.Factories;
using Transport.Interfaces;
using Transport.Services;

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