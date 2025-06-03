using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Bitcoin.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Interface for adapters that convert domain to_local output info models to infrastructure to_local outputs.
/// </summary>
public interface IToLocalOutputAdapter : IOutputAdapter<ToLocalOutputInfo, ToLocalOutput>
{
}
