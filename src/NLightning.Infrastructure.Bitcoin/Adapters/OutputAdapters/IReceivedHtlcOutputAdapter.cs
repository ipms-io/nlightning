using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Bitcoin.Outputs;

namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

/// <summary>
/// Interface for adapters that convert domain received HTLC output info models to infrastructure received HTLC outputs.
/// </summary>
public interface IReceivedHtlcOutputAdapter : IOutputAdapter<ReceivedHtlcOutputInfo, ReceivedHtlcOutput>
{
}
