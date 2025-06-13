using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Infrastructure;

using Crypto.Hashes;
using Domain.Crypto.Hashes;
using Domain.Node.Interfaces;
using Domain.Protocol.Interfaces;
using Node.Factories;
using Protocol.Factories;
using Protocol.Services;
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
        services.AddSingleton<IPeerServiceFactory, PeerServiceFactory>();
        services.AddSingleton<ITcpService, TcpService>();
        services.AddSingleton<ISha256, Sha256>();
        services.AddSingleton<ITransportServiceFactory, TransportServiceFactory>();

        // Transient services (new instance each time requested)
        services.AddTransient<IPingPongService, PingPongService>();

        return services;
    }
}