using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Bitcoin.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Interface for adapters that convert domain offered HTLC output info models to infrastructure offered HTLC outputs.
/// </summary>
public interface IOfferedHtlcOutputAdapter : IOutputAdapter<OfferedHtlcOutputInfo, OfferedHtlcOutput>
{
}
