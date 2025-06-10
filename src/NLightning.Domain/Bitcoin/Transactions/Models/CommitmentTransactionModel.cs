using NLightning.Domain.Bitcoin.Transactions.Interfaces;
using NLightning.Domain.Bitcoin.Transactions.Outputs;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Domain.Bitcoin.Transactions.Models;

/// <summary>
/// Represents a commitment transaction in the domain model.
/// This class encapsulates the logical structure of a Lightning Network commitment transaction
/// as defined by BOLT specifications, without dependencies on specific Bitcoin libraries.
/// </summary>
public class CommitmentTransactionModel
{
    /// <summary>
    /// Gets the funding outpoint that this commitment transaction spends.
    /// </summary>
    public FundingOutputInfo FundingOutput { get; }

    /// <summary>
    /// Gets the commitment number for this transaction.
    /// </summary>
    public CommitmentNumber CommitmentNumber { get; }

    /// <summary>
    /// Gets or sets the transaction ID after the transaction is constructed.
    /// </summary>
    public TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets the to_local output, if present.
    /// </summary>
    public ToLocalOutputInfo? ToLocalOutput { get; }

    /// <summary>
    /// Gets the to_remote output, if present.
    /// </summary>
    public ToRemoteOutputInfo? ToRemoteOutput { get; }

    /// <summary>
    /// Gets the local anchor output, if present.
    /// </summary>
    public AnchorOutputInfo? LocalAnchorOutput { get; }

    /// <summary>
    /// Gets the remote anchor output, if present.
    /// </summary>
    public AnchorOutputInfo? RemoteAnchorOutput { get; }

    /// <summary>
    /// Gets the list of offered HTLC outputs.
    /// </summary>
    public IReadOnlyList<OfferedHtlcOutputInfo> OfferedHtlcOutputs { get; }

    /// <summary>
    /// Gets the list of received HTLC outputs.
    /// </summary>
    public IReadOnlyList<ReceivedHtlcOutputInfo> ReceivedHtlcOutputs { get; }

    /// <summary>
    /// Gets the total fee for this transaction.
    /// </summary>
    public LightningMoney Fee { get; }

    /// <summary>
    /// Creates a new instance of CommitmentTransactionModel.
    /// </summary>
    public CommitmentTransactionModel(CommitmentNumber commitmentNumber, LightningMoney fee,
                                      FundingOutputInfo fundingOutput, AnchorOutputInfo? localAnchorOutput = null,
                                      AnchorOutputInfo? remoteAnchorOutput = null,
                                      ToLocalOutputInfo? toLocalOutput = null,
                                      ToRemoteOutputInfo? toRemoteOutput = null,
                                      IEnumerable<OfferedHtlcOutputInfo>? offeredHtlcOutputs = null,
                                      IEnumerable<ReceivedHtlcOutputInfo>? receivedHtlcOutputs = null)
    {
        if (fundingOutput.TransactionId is null || fundingOutput.TransactionId.Value == TxId.Zero)
            throw new ArgumentException("Funding output must have a valid transaction ID.", nameof(fundingOutput));

        FundingOutput = fundingOutput;
        CommitmentNumber = commitmentNumber;
        Fee = fee;
        ToLocalOutput = toLocalOutput;
        ToRemoteOutput = toRemoteOutput;
        LocalAnchorOutput = localAnchorOutput;
        RemoteAnchorOutput = remoteAnchorOutput;
        OfferedHtlcOutputs = (offeredHtlcOutputs ?? []).ToList();
        ReceivedHtlcOutputs = (receivedHtlcOutputs ?? []).ToList();
    }

    /// <summary>
    /// Gets the Bitcoin locktime for this commitment transaction, derived from the commitment number.
    /// </summary>
    public BitcoinLockTime GetLockTime() => CommitmentNumber.CalculateLockTime();

    /// <summary>
    /// Gets the Bitcoin sequence for this commitment transaction, derived from the commitment number.
    /// </summary>
    public BitcoinSequence GetSequence() => CommitmentNumber.CalculateSequence();

    /// <summary>
    /// Gets all outputs of this commitment transaction.
    /// </summary>
    public IEnumerable<IOutputInfo> GetAllOutputs()
    {
        if (ToLocalOutput != null) yield return ToLocalOutput;
        if (ToRemoteOutput != null) yield return ToRemoteOutput;
        if (LocalAnchorOutput != null) yield return LocalAnchorOutput;
        if (RemoteAnchorOutput != null) yield return RemoteAnchorOutput;

        foreach (var output in OfferedHtlcOutputs) yield return output;
        foreach (var output in ReceivedHtlcOutputs) yield return output;
    }
}