using Microsoft.Extensions.DependencyInjection;
using NLightning.Domain.Protocol.Factories;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Infrastructure.Bitcoin.Crypto.Functions;
using NLightning.Infrastructure.Bitcoin.Factories;
using NLightning.Infrastructure.Bitcoin.Services;
using NLightning.Infrastructure.Crypto.Interfaces;
using NLightning.Infrastructure.Protocol.Factories;

namespace NLightning.Infrastructure.Bitcoin;

using Application.Bitcoin.Interfaces;
using Builders;

/// <summary>
/// Extension methods for setting up Bitcoin infrastructure services in an IServiceCollection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Bitcoin infrastructure services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddBitcoinInfrastructure(this IServiceCollection services)
    {
        // Register Singletons
        services.AddSingleton<IChannelKeySetFactory, ChannelKeySetFactory>();
        services.AddSingleton<IEcdh, Ecdh>();
        services.AddSingleton<IKeyDerivationService, KeyDerivationService>();
        services.AddSingleton<ITlvConverterFactory, TlvConverterFactory>();
        
        // Register Scoped Services
        services.AddScoped<ICommitmentTransactionBuilder, CommitmentTransactionBuilder>();
        
        return services;
    }
}
