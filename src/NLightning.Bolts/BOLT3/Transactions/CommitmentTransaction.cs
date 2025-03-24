using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Interfaces;
using Common.Managers;
using Constants;
using Outputs;
using Types;

/// <summary>
/// Represents a commitment transaction.
/// </summary>
public class CommitmentTransaction : BaseTransaction
{
    private readonly bool _isChannelFunder;
    private readonly IList<OfferedHtlcOutput> _offeredHtlcOutputs = [];
    private readonly IList<ReceivedHtlcOutput> _receivedHtlcOutputs = [];
    private readonly LightningMoney _toFunderAmount;

    public ToLocalOutput ToLocalOutput { get; }
    public ToRemoteOutput ToRemoteOutput { get; }
    public ToAnchorOutput? LocalAnchorOutput { get; }
    public ToAnchorOutput? RemoteAnchorOutput { get; }

    public CommitmentNumber CommitmentNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitmentTransaction"/> class.
    /// </summary>
    /// <param name="fundingCoin">The funding coin.</param>
    /// <param name="localPaymentBasepoint">The local public key.</param>
    /// <param name="remotePaymentBasepoint">The remote public key.</param>
    /// <param name="localDelayedPubKey">The local delayed public key.</param>
    /// <param name="revocationPubKey">The revocation public key.</param>
    /// <param name="toLocalAmount">The amount for the to_local output in satoshis.</param>
    /// <param name="toRemoteAmount">The amount for the to_remote output in satoshis.</param>
    /// <param name="toSelfDelay">The to_self_delay in blocks.</param>
    /// <param name="commitmentNumber">The commitment number object.</param>
    /// <param name="isChannelFunder">Indicates if the local node is the channel funder.</param>
    internal CommitmentTransaction(Coin fundingCoin, PubKey localPaymentBasepoint, PubKey remotePaymentBasepoint,
                                   PubKey localDelayedPubKey, PubKey revocationPubKey, LightningMoney toLocalAmount,
                                   LightningMoney toRemoteAmount, uint toSelfDelay, CommitmentNumber commitmentNumber,
                                   bool isChannelFunder)
        : base(TransactionConstants.COMMITMENT_TRANSACTION_VERSION, (fundingCoin, commitmentNumber.CalculateSequence()))
    {
        ArgumentNullException.ThrowIfNull(localPaymentBasepoint);
        ArgumentNullException.ThrowIfNull(remotePaymentBasepoint);
        ArgumentNullException.ThrowIfNull(localDelayedPubKey);
        ArgumentNullException.ThrowIfNull(revocationPubKey);

        if (toLocalAmount.IsZero && toRemoteAmount.IsZero)
        {
            throw new ArgumentException("Both toLocalAmount and toRemoteAmount cannot be zero.");
        }

        _isChannelFunder = isChannelFunder;
        CommitmentNumber = commitmentNumber;

        // Set locktime
        SetLockTime(commitmentNumber.CalculateLockTime());

        // Set funder amount
        var localAmount = LightningMoney.Zero;
        var remoteAmount = LightningMoney.Zero;
        if (_isChannelFunder)
        {
            // localAmount will be calculated later
            _toFunderAmount = toLocalAmount;
            remoteAmount = toRemoteAmount;
        }
        else
        {
            // remoteAmount will be calculated later
            _toFunderAmount = toRemoteAmount;
            localAmount = toLocalAmount;
        }

        // to_local output
        ToLocalOutput = new ToLocalOutput(localDelayedPubKey, revocationPubKey, toSelfDelay, localAmount);
        if (toLocalAmount >= ConfigManager.Instance.DustLimitAmount) // Dust limit in satoshis
        {
            AddOutput(ToLocalOutput);
        }

        // to_remote output
        ToRemoteOutput = new ToRemoteOutput(remotePaymentBasepoint, remoteAmount);
        if (toRemoteAmount >= ConfigManager.Instance.DustLimitAmount) // Dust limit in satoshis
        {
            AddOutput(ToRemoteOutput);
        }

        if (!ConfigManager.Instance.IsOptionAnchorOutput || ConfigManager.Instance.AnchorAmount == LightningMoney.Zero)
        {
            return;
        }

        // Local anchor output
        LocalAnchorOutput = new ToAnchorOutput(localPaymentBasepoint, ConfigManager.Instance.AnchorAmount);
        AddOutput(LocalAnchorOutput);

        // Remote anchor output
        RemoteAnchorOutput = new ToAnchorOutput(remotePaymentBasepoint, ConfigManager.Instance.AnchorAmount);
        AddOutput(RemoteAnchorOutput);
    }

    internal void SignTransaction(IFeeService feeService, params BitcoinSecret[] secrets)
    {
        SignAndFinalizeTransaction(feeService, secrets);

        // Deduct the fee from initiator
        var toFunderAmount = LightningMoney.Zero;
        if (CalculatedFee <= _toFunderAmount)
        {
            toFunderAmount = _toFunderAmount - CalculatedFee;
        }

        if (_isChannelFunder)
        {
            RemoveOutput(ToLocalOutput);
            if (toFunderAmount >= ConfigManager.Instance.DustLimitAmount)
            {
                // Set amount
                ToLocalOutput.Amount = toFunderAmount;

                // Add the new output
                ToLocalOutput.Index = (uint)AddOutput(ToLocalOutput);
            }

            ToRemoteOutput.Index = (uint)OUTPUTS.IndexOf(ToRemoteOutput);
        }
        else
        {
            RemoveOutput(ToRemoteOutput);
            if (toFunderAmount >= ConfigManager.Instance.DustLimitAmount)
            {
                // Set amount
                ToRemoteOutput.Amount = toFunderAmount;

                // Add the new output
                ToRemoteOutput.Index = (uint)AddOutput(ToRemoteOutput);
            }

            ToLocalOutput.Index = (uint)OUTPUTS.IndexOf(ToLocalOutput);
        }

        SignTransactionWithExistingKeys();

        ToRemoteOutput.TxId = TxId;
        ToLocalOutput.TxId = TxId;

        if (ConfigManager.Instance.IsOptionAnchorOutput)
        {
            LocalAnchorOutput!.TxId = TxId;
            RemoteAnchorOutput!.TxId = TxId;

            LocalAnchorOutput.Index = (uint)OUTPUTS.IndexOf(LocalAnchorOutput);
            RemoteAnchorOutput.Index = (uint)OUTPUTS.IndexOf(RemoteAnchorOutput);
        }

        // Deal with HTLCs
        foreach (var offeredHtlcOutput in _offeredHtlcOutputs)
        {
            offeredHtlcOutput.TxId = TxId;
            offeredHtlcOutput.Index = (uint)OUTPUTS.IndexOf(offeredHtlcOutput); ;
        }

        foreach (var receivedHtlcOutput in _receivedHtlcOutputs)
        {
            receivedHtlcOutput.TxId = TxId;
            receivedHtlcOutput.Index = (uint)OUTPUTS.IndexOf(receivedHtlcOutput); ;
        }
    }

    internal Transaction GetSignedTransaction()
    {
        if (Finalized)
        {
            return FinalizedTransaction;
        }

        throw new InvalidOperationException("You have to sign and finalize the transaction first.");
    }

    public void AddOfferedHtlcOutputAndUpdate(OfferedHtlcOutput offeredHtlcOutput)
    {
        // Add output to lists
        _offeredHtlcOutputs.Add(offeredHtlcOutput);
        AddOutput(offeredHtlcOutput);
    }

    public void AddReceivedHtlcOutputAndUpdate(ReceivedHtlcOutput receivedHtlcOutput)
    {
        _receivedHtlcOutputs.Add(receivedHtlcOutput);
        AddOutput(receivedHtlcOutput);
    }

    public void AppendRemoteSignatureAndSign(ECDSASignature remoteSignature, PubKey remotePubKey)
    {
        AppendRemoteSignatureToTransaction(new TransactionSignature(remoteSignature), remotePubKey);
        SignTransactionWithExistingKeys();
    }
}