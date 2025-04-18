using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLightning.Bolts.BOLT1.Factories;
using NLightning.Bolts.BOLT1.Managers;
using NLightning.Common.Factories;
using NLightning.Common.Interfaces;
using NLightning.Common.Options;
using NLightning.Common.Services;
using NLightning.NLTG.Interfaces;
using NLightning.NLTG.Services;

namespace NLightning.NLTG.Extensions;

public static class NltgServiceExtensions
{
    /// <summary>
    /// Registers all NLTG application services for dependency injection
    /// </summary>
    public static IHostBuilder ConfigureNltgServices(this IHostBuilder hostBuilder)
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
            services.AddSingleton<IFeeService, FeeService>();
            services.AddSingleton<IMessageFactory, MessageFactory>();
            services.AddSingleton<IMessageServiceFactory, MessageServiceFactory>();
            services.AddSingleton<IPeerFactory, PeerFactory>();
            services.AddSingleton<IPeerManager, PeerManager>();
            services.AddSingleton<IPingPongServiceFactory, PingPongServiceFactory>();
            services.AddSingleton<ITcpListenerService, TcpListenerService>();
            services.AddSingleton<ITransportServiceFactory, TransportServiceFactory>();

            // Scoped services (one instance per scope)

            // Transient services (new instance each time)

            // Register options with values from configuration
            services.AddOptions<NodeOptions>()
                .Configure<IConfiguration>((options, config) =>
                {
                    // Default values are set in the NodeOptions class itself
                    // Override with configuration values if present
                    config.GetSection("Node").Bind(options);
                })
                .ValidateOnStart(); // Ensures validation happens at startup
        });
    }
}