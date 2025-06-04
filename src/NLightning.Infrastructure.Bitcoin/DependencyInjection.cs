using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Infrastructure.Bitcoin;

using Domain.Protocol.Factories;
using Domain.Protocol.Interfaces;
using Crypto.Functions;
using Services;
using Infrastructure.Crypto.Interfaces;
using Protocol.Factories;
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
        services.AddSingleton<ICommitmentKeyDerivationService, CommitmentKeyDerivationService>();
        services.AddSingleton<ICommitmentTransactionBuilder, CommitmentTransactionBuilder>();
        services.AddSingleton<IEcdh, Ecdh>();
        services.AddSingleton<IFundingOutputBuilder, FundingOutputBuilder>();
        services.AddSingleton<IKeyDerivationService, KeyDerivationService>();
        services.AddSingleton<ITlvConverterFactory, TlvConverterFactory>();

        return services;
    }
}