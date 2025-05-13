using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLightning.Application.Interfaces;
using NLightning.Application.Interfaces.Services;
using NLightning.Application.NLTG.Managers;
using NLightning.Application.Options;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization;
using NLightning.Infrastructure.Bitcoin.Services;
using NLightning.Infrastructure.Node;
using NLightning.Infrastructure.Node.Interfaces;
using NLightning.Infrastructure.Node.Managers;
using NLightning.Infrastructure.Protocol;
using NLightning.Infrastructure.Protocol.Factories;
using NLightning.Infrastructure.Serialization;

namespace NLightning.Application.NLTG.Extensions;

using Bolts.BOLT1.Factories;
using Common.Interfaces;
using Common.Options;
using Interfaces;
using Services;

public static class NltgServiceExtensions
{
    /// <summary>
    /// Registers all NLTG application services for dependency injection
    /// </summary>
    public static IHostBuilder ConfigureNltgServices(this IHostBuilder hostBuilder, SecureKeyManager secureKeyManager)
    {
        return hostBuilder.ConfigureServices((hostContext, services) =>
        {
            // Get configuration
            var configuration = hostContext.Configuration;

            // Register configuration as a service
            services.AddSingleton(configuration);

            // Register the main daemon service
            services.AddHostedService<NltgDaemonService>();

            // Add HttpClient for FeeService with configuration
            services.AddHttpClient<IFeeService, FeeService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Register application services
            // Singleton services (one instance throughout the application)
            services.AddSingleton<IEndianConverter, EndianConverterAdapter>();
            services.AddSingleton<IFeeService, FeeService>();
            services.AddSingleton<IMessageFactory, MessageFactory>();
            services.AddSingleton<IMessageServiceFactory, MessageServiceFactory>();
            services.AddSingleton<IPeerFactory, PeerFactory>();
            services.AddSingleton<IPeerManager, PeerManager>();
            services.AddSingleton<IPingPongServiceFactory, PingPongServiceFactory>();
            services.AddSingleton<ISecureKeyManager>(secureKeyManager);
            services.AddSingleton<ITcpListenerService, TcpListenerService>();
            services.AddSingleton<ITransportServiceFactory, TransportServiceFactory>();

            // Scoped services (one instance per scope)

            // Transient services (new instance each time)

            // Register options with values from configuration
            services.AddOptions<FeeEstimationOptions>().BindConfiguration("FeeEstimation").ValidateOnStart();
            services.AddOptions<NodeOptions>()
                .BindConfiguration("Node")
                .PostConfigure(options =>
                {
                    var configuredAddresses = configuration.GetSection("Node:ListenAddresses").Get<string[]?>();
                    if (configuredAddresses is { Length: > 0 })
                    {
                        options.ListenAddresses = configuredAddresses.ToList();
                    }
                })
                .ValidateOnStart();
        });
    }
}