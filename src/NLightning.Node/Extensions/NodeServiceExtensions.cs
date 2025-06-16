using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLightning.Application;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Bitcoin.Transactions.Factories;
using NLightning.Domain.Bitcoin.Transactions.Interfaces;
using NLightning.Domain.Channels.Factories;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Crypto.Hashes;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Infrastructure;
using NLightning.Infrastructure.Bitcoin;
using NLightning.Infrastructure.Bitcoin.Builders;
using NLightning.Infrastructure.Bitcoin.Managers;
using NLightning.Infrastructure.Bitcoin.Options;
using NLightning.Infrastructure.Bitcoin.Services;
using NLightning.Infrastructure.Bitcoin.Signers;
using NLightning.Infrastructure.Persistence;
using NLightning.Infrastructure.Repositories;
using NLightning.Infrastructure.Serialization;

namespace NLightning.Node.Extensions;

using Domain.Node.Options;
using Services;

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

            // Singleton services (one instance throughout the application)
            services.AddSingleton<ISecureKeyManager>(secureKeyManager);
            services.AddSingleton<IChannelFactory>(sp =>
            {
                var feeService = sp.GetRequiredService<IFeeService>();
                var lightningSigner = sp.GetRequiredService<ILightningSigner>();
                var nodeOptions = sp.GetRequiredService<IOptions<NodeOptions>>().Value;
                var sha256 = sp.GetRequiredService<ISha256>();
                return new ChannelFactory(feeService, lightningSigner, nodeOptions, sha256);
            });
            services.AddSingleton<ICommitmentTransactionModelFactory, CommitmentTransactionModelFactory>();

            // Add the Signer
            services.AddSingleton<ILightningSigner>(serviceProvider =>
            {
                var fundingOutputBuilder = serviceProvider.GetRequiredService<IFundingOutputBuilder>();
                var keyDerivationService = serviceProvider.GetRequiredService<IKeyDerivationService>();
                var logger = serviceProvider.GetRequiredService<ILogger<LocalLightningSigner>>();
                var nodeOptions = serviceProvider.GetRequiredService<IOptions<NodeOptions>>().Value;

                // Create the signer with the correct network
                return new LocalLightningSigner(fundingOutputBuilder, keyDerivationService, logger, nodeOptions,
                                                secureKeyManager);
            });

            // Add the Application services
            services.AddApplicationServices();

            // Add the Infrastructure services
            services.AddBitcoinInfrastructure();
            services.AddInfrastructureServices();
            services.AddPersistenceInfrastructureServices(configuration);
            services.AddRepositoriesInfrastructureServices();
            services.AddSerializationInfrastructureServices();

            // Scoped services (one instance per scope)

            // Transient services (new instance each time)

            // Register options with values from configuration
            services.AddOptions<BitcoinOptions>().BindConfiguration("Bitcoin").ValidateOnStart();
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
                             options.BitcoinNetwork = new BitcoinNetwork(networkString);
                         }

                         options.Features.ChainHashes = [options.BitcoinNetwork.ChainHash];
                     })
                    .ValidateOnStart();
        });
    }
}