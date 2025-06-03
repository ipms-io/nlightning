using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Bitcoin.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Interface for adapters that convert domain anchor output info models to infrastructure anchor outputs.
/// </summary>
public interface IAnchorOutputAdapter : IOutputAdapter<AnchorOutputInfo, ToAnchorOutput>
{
}
