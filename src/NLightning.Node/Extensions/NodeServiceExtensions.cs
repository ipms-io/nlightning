using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLightning.Infrastructure.Bitcoin.Options;

namespace NLightning.Node.Extensions;

using Application.Factories;
using Application.Factories.Interfaces;
using Application.Managers;
using Application.Node.Services.Interfaces;
using Domain.Bitcoin.Factories;
using Domain.Bitcoin.Services;
using Domain.Node.Options;
using Domain.Protocol.Factories;
using Domain.Protocol.Managers;
using Domain.Protocol.Services;
using Domain.Protocol.Signers;
using Domain.ValueObjects;
using Infrastructure.Bitcoin.Factories;
using Infrastructure.Node.Interfaces;
using Infrastructure.Node.Managers;
using Infrastructure.Protocol.Factories;
using Infrastructure.Protocol.Services;
using Infrastructure.Transport.Factories;
using Interfaces;
using Managers;
using Services;
using Signers;

public static class NodeServiceExtensions
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
            services.AddSingleton<IChannelFactory, ChannelFactory>();
            services.AddSingleton<IChannelManager, ChannelManager>();
            services.AddSingleton<ICommitmentTransactionFactory, CommitmentTransactionFactory>();
            services.AddSingleton<IFeeService, FeeService>();
            services.AddSingleton<IFundingTransactionFactory, FundingTransactionFactory>();
            services.AddSingleton<IKeyDerivationService, KeyDerivationService>();
            services.AddSingleton<IMessageFactory, MessageFactory>();
            services.AddSingleton<IMessageServiceFactory, MessageServiceFactory>();
            services.AddSingleton<IPeerServiceFactory, PeerServiceFactory>();
            services.AddSingleton<IPeerManager, PeerManager>();
            services.AddSingleton<IPingPongServiceFactory, PingPongServiceFactory>();
            services.AddSingleton<ISecureKeyManager>(secureKeyManager);
            services.AddSingleton<ITcpListenerService, TcpListenerService>();
            services.AddSingleton<ITransportServiceFactory, TransportServiceFactory>();

            // Add the Signer
            services.AddSingleton<ILightningSigner>(serviceProvider =>
            {
                var keyDerivationService = serviceProvider.GetRequiredService<IKeyDerivationService>();
                var logger = serviceProvider.GetRequiredService<ILogger<LocalLightningSigner>>();

                // Create the signer with the correct network
                return new LocalLightningSigner(keyDerivationService, logger, secureKeyManager);
            });

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

                    var networkString = configuration.GetValue<string>("Node:Network");
                    if (!string.IsNullOrWhiteSpace(networkString))
                    {
                        options.Network = new Network(networkString);
                    }
                })
                .ValidateOnStart();
        });
    }
}