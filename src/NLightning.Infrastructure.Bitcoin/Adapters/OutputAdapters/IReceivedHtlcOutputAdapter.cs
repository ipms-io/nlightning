namespace NLightning.Infrastructure.Bitcoin.Adapters.OutputAdapters;

using Domain.Transactions.Outputs;
using Outputs;

/// <summary>
/// Interface for adapters that convert domain received HTLC output info models to infrastructure received HTLC outputs.
/// </summary>
public interface IReceivedHtlcOutputAdapter : IOutputAdapter<ReceivedHtlcOutputInfo, ReceivedHtlcOutput>
{
}