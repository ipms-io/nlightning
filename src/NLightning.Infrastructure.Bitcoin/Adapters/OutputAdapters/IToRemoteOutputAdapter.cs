namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

using Domain.Bitcoin.Transactions.Outputs;
using Outputs;

/// <summary>
/// Interface for adapters that convert domain to_remote output info models to infrastructure to_remote outputs.
/// </summary>
public interface IToRemoteOutputAdapter : IOutputAdapter<ToRemoteOutputInfo, ToRemoteOutput>
{
}