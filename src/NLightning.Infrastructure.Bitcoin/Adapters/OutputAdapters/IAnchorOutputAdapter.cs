using NLightning.Domain.Bitcoin.Transactions.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

using Outputs;

/// <summary>
/// Interface for adapters that convert domain anchor output info models to infrastructure anchor outputs.
/// </summary>
public interface IAnchorOutputAdapter : IOutputAdapter<AnchorOutputInfo, ToAnchorOutput>
{
}