namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

using Domain.Transactions.Outputs;
using Outputs;

/// <summary>
/// Interface for adapters that convert domain offered HTLC output info models to infrastructure offered HTLC outputs.
/// </summary>
public interface IOfferedHtlcOutputAdapter : IOutputAdapter<OfferedHtlcOutputInfo, OfferedHtlcOutput>
{
}