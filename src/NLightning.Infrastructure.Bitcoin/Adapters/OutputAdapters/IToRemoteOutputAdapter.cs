using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Bitcoin.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Interface for adapters that convert domain to_remote output info models to infrastructure to_remote outputs.
/// </summary>
public interface IToRemoteOutputAdapter : IOutputAdapter<ToRemoteOutputInfo, ToRemoteOutput>
{
}
