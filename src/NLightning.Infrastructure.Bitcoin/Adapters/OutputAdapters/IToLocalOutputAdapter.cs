namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

using Domain.Transactions.Outputs;
using Outputs;

/// <summary>
/// Interface for adapters that convert domain to_local output info models to infrastructure to_local outputs.
/// </summary>
public interface IToLocalOutputAdapter : IOutputAdapter<ToLocalOutputInfo, ToLocalOutput>
{
}