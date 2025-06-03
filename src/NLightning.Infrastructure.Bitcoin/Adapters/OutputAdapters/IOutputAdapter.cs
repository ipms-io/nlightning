using NLightning.Domain.Transactions.Interfaces;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Base interface for adapters that convert domain output info models to infrastructure outputs.
/// </summary>
/// <typeparam name="TDomainOutput">The type of domain output info model.</typeparam>
/// <typeparam name="TInfraOutput">The type of infrastructure output.</typeparam>
public interface IOutputAdapter<TDomainOutput, TInfraOutput> where TDomainOutput : IOutputInfo
{
    /// <summary>
    /// Creates an infrastructure output from a domain output info model.
    /// </summary>
    /// <param name="outputInfo">The domain output info model.</param>
    /// <returns>The infrastructure output.</returns>
    TInfraOutput CreateOutput(TDomainOutput outputInfo);
}
