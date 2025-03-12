using NBitcoin;
using NLightning.Bolts.BOLT3.Comparers;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Managers;
using Constants;
using Outputs;

/// <summary>
/// Represents a commitment transaction.
/// </summary>
public class CommitmentTransaction: Transaction
{
    private readonly IList<OutputBase> _outputList = [];
    private readonly IList<OfferedHtlcOutput> _offeredHtlcOutputs = [];
    private readonly IList<ReceivedHtlcOutput> _receivedHtlcOutputs = [];

    public PubKey LocalPubKey { get; }
    public ToLocalOutput ToLocalOutput => _outputList[0] as ToLocalOutput ?? throw new InvalidCastException("Output 0 was not type ToLocalOutput");
    public ToRemoteOutput ToRemoteOutput => _outputList[1] as ToRemoteOutput ?? throw new InvalidCastException("Output 1 was not type ToRemoteOutput");
    public ToAnchorOutput? LocalAnchorOutput => ConfigManager.Instance.IsOptionAnchorOutput 
        ? _outputList[2] as ToAnchorOutput ?? throw new InvalidCastException("Output 2 was not type ToAnchorOutput")
        : null;
    public ToAnchorOutput? RemoteAnchorOutput => ConfigManager.Instance.IsOptionAnchorOutput
        ? _outputList[3] as ToAnchorOutput ?? throw new InvalidCastException("Output 3 was not type ToAnchorOutput")
        : null;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitmentTransaction"/> class.
    /// </summary>
    /// <param name="fundingTxId">The funding transaction ID.</param>
    /// <param name="fundingOutputIndex">The index of the funding output.</param>
    /// <param name="localPubKey">The local public key.</param>
    /// <param name="remotePubKey">The remote public key.</param>
    /// <param name="localDelayedPubKey">The local delayed public key.</param>
    /// <param name="revocationPubKey">The revocation public key.</param>
    /// <param name="toLocalAmount">The amount for the to_local output in satoshis.</param>
    /// <param name="toRemoteAmount">The amount for the to_remote output in satoshis.</param>
    /// <param name="toSelfDelay">The to_self_delay in blocks.</param>
    /// <param name="obscuredCommitmentNumber">The obscured commitment number.</param>
    public CommitmentTransaction(
        uint256 fundingTxId,
        uint fundingOutputIndex,
        PubKey localPubKey,
        PubKey remotePubKey,
        PubKey localDelayedPubKey,
        PubKey revocationPubKey,
        Money toLocalAmount,
        Money toRemoteAmount,
        uint toSelfDelay,
        ulong obscuredCommitmentNumber)
        // byte[] pubkey1Signature,
        // byte[] pubkey2Signature)
    {
        LocalPubKey = localPubKey;
        
        // Set version and locktime
        Version = TransactionConstants.COMMITMENT_TRANSACTION_VERSION;
        LockTime = new LockTime((0x20 << 24) | (uint)(obscuredCommitmentNumber & 0xFFFFFF));

        // Add the funding input
        var outpoint = new OutPoint(fundingTxId, fundingOutputIndex);
        var sequence = new Sequence(0x80 << 24) | (uint)((obscuredCommitmentNumber >> 24) & 0xFFFFFF);
        Inputs.Add(outpoint, null, null, sequence);

        // to_local output
        if (toLocalAmount >= ConfigManager.Instance.DustLimitAmountMoney) // Dust limit in satoshis
        {
            var toLocalOutput = new ToLocalOutput(localDelayedPubKey, revocationPubKey, toSelfDelay, toLocalAmount);
            _outputList.Add(toLocalOutput);
            Outputs.Add(toLocalOutput.ToTxOut());
        }

        // to_remote output
        if (toRemoteAmount >= ConfigManager.Instance.DustLimitAmountMoney) // Dust limit in satoshis
        {
            var toRemoteOutput = new ToRemoteOutput(remotePubKey, toRemoteAmount);
            _outputList.Add(toRemoteOutput);
            Outputs.Add(toRemoteOutput.ToTxOut());
        }

        if (!ConfigManager.Instance.IsOptionAnchorOutput || ConfigManager.Instance.AnchorAmountSats == 0)
        {
            return;
        }

        // Local anchor output
        var localAnchor = new ToAnchorOutput(localPubKey, ConfigManager.Instance.AnchorAmountSats);
        _outputList.Add(localAnchor);
        Outputs.Add(localAnchor.ToTxOut());

        // Remote anchor output
        var remoteAnchor = new ToAnchorOutput(remotePubKey, ConfigManager.Instance.AnchorAmountSats);
        _outputList.Add(remoteAnchor);
        Outputs.Add(remoteAnchor.ToTxOut());
    }

    public void Sign(ReadOnlySpan<byte> pubkey1Signature, ReadOnlySpan<byte> pubkey2Signature)
    {
        Inputs[0].WitScript = new WitScript(OpcodeType.OP_0, Op.GetPushOp(pubkey1Signature.ToArray()), Op.GetPushOp(pubkey2Signature.ToArray()));
    }

    public void AddOfferedHtlcOutputAndUpdate(OfferedHtlcOutput offeredHtlcOutput)
    {
        // Add output to lists
        _offeredHtlcOutputs.Add(offeredHtlcOutput);
        _outputList.Add(offeredHtlcOutput);
        
        // Clear TxOuts
        Outputs.Clear();
        
        // Add ordered outputs
        Outputs.AddRange(
            _outputList.OrderBy(htlc => htlc, TransactionOutputComparer.Instance)
                       .Select(htlc => htlc.ToTxOut())
        );
    }

    public void AddReceivedHtlcOutputAndUpdate(ReceivedHtlcOutput receivedHtlcOutput)
    {
        _receivedHtlcOutputs.Add(receivedHtlcOutput);
        _outputList.Add(receivedHtlcOutput);
        
        // Clear TxOuts
        Outputs.Clear();
        
        // Add ordered outputs
        Outputs.AddRange(
            _outputList.OrderBy(htlc => htlc, TransactionOutputComparer.Instance)
                .Select(htlc => htlc.ToTxOut())
        );
    }
}