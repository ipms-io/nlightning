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
    private const int INPUT_WEIGHT = WeightConstants.WITNESS_HEADER
                                     + WeightConstants.MULTISIG_WITNESS_WEIGHT
                                     + (4 * WeightConstants.P2WSH_INTPUT_WEIGHT);

    private readonly bool _isChannelFunder;
    private readonly IList<OfferedHtlcOutput> _offeredHtlcOutputs = [];
    private readonly IList<ReceivedHtlcOutput> _receivedHtlcOutputs = [];

    private LightningMoney _toFunderAmount;

    public ToLocalOutput ToLocalOutput { get; }
    public ToRemoteOutput ToRemoteOutput { get; }
    public ToAnchorOutput? LocalAnchorOutput { get; private set; }
    public ToAnchorOutput? RemoteAnchorOutput { get; private set; }

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

    internal void ConstructTransaction(IFeeService feeService)
    {
        var currentFeePerKw = feeService.GetCachedFeeRatePerKw();

        // Calculate base fee
        var outputWeight = CalculateOutputWeight();
        var calculatedFee = (outputWeight + INPUT_WEIGHT) * currentFeePerKw.Satoshi / 1000L;
        if (CalculatedFee.Satoshi != calculatedFee)
        {
            CalculatedFee.Satoshi = calculatedFee;
        }

        // Deduct base fee from the funder amount
        if (CalculatedFee > _toFunderAmount)
        {
            _toFunderAmount = LightningMoney.Zero;
        }
        else
        {
            _toFunderAmount -= CalculatedFee;
        }

        // Deduct anchor fee from the funder amount
        if (ConfigManager.Instance.IsOptionAnchorOutput && !_toFunderAmount.IsZero)
        {
            _toFunderAmount -= ConfigManager.Instance.AnchorAmount;
            _toFunderAmount -= ConfigManager.Instance.AnchorAmount;
        }

        // Trim Local and Remote outputs
        if (_isChannelFunder)
        {
            SetLocalAndRemoteAmounts(ToLocalOutput, ToRemoteOutput);
        }
        else
        {
            SetLocalAndRemoteAmounts(ToRemoteOutput, ToLocalOutput);
        }

        // Trim HTLCs
        if (ConfigManager.Instance.MustTrimHtlcOutputs)
        {
            var offeredHtlcWeight = ConfigManager.Instance.IsOptionAnchorOutput
                ? WeightConstants.HTLC_TIMEOUT_WEIGHT_ANCHORS
                : WeightConstants.HTLC_TIMEOUT_WEIGHT_NO_ANCHORS;
            var offeredHtlcFee = offeredHtlcWeight * currentFeePerKw.Satoshi / 1000L;
            foreach (var offeredHtlcOutput in _offeredHtlcOutputs)
            {
                var htlcAmount = offeredHtlcOutput.Amount - offeredHtlcFee;
                if (htlcAmount < ConfigManager.Instance.DustLimitAmount)
                {
                    RemoveOutput(offeredHtlcOutput);
                }
            }

            var receivedHtlcWeight = ConfigManager.Instance.IsOptionAnchorOutput
                ? WeightConstants.HTLC_SUCCESS_WEIGHT_ANCHORS
                : WeightConstants.HTLC_SUCCESS_WEIGHT_NO_ANCHORS;
            var receivedHtlcFee = receivedHtlcWeight * currentFeePerKw.Satoshi / 1000L;
            foreach (var receivedHtlcOutput in _receivedHtlcOutputs)
            {
                var htlcAmount = receivedHtlcOutput.Amount - receivedHtlcFee;
                if (htlcAmount < ConfigManager.Instance.DustLimitAmount)
                {
                    RemoveOutput(receivedHtlcOutput);
                }
            }
        }

        // Add anchors if needed
        if (ConfigManager.Instance.IsOptionAnchorOutput && !OUTPUTS.Any(o => o is HtlcOutput))
        {
            if (ToLocalOutput.Amount.IsZero)
            {
                RemoveOutput(ToLocalOutput);
            }

            if (ToRemoteOutput.Amount.IsZero)
            {
                RemoveOutput(ToRemoteOutput);
            }
        }

        // Order Outputs
        AddOrderedOutputsToTransaction();
    }

    internal new void SignTransaction(params BitcoinSecret[] secrets)
    {
        base.SignTransaction(secrets);

        SetTxId();
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

        SetTxId();
    }

    private void SetLocalAndRemoteAmounts(BaseOutput funderOutput, BaseOutput otherOutput)
    {
        if (_toFunderAmount >= ConfigManager.Instance.DustLimitAmount)
        {
            if (_toFunderAmount != funderOutput.Amount)
            {
                // Remove old output
                RemoveOutput(funderOutput);

                // Set amount
                funderOutput.Amount = _toFunderAmount;

                // Add new output
                funderOutput.Index = (uint)AddOutput(funderOutput);
            }
        }
        else
        {
            RemoveOutput(funderOutput);
            funderOutput.Amount = LightningMoney.Zero;
        }

        RemoveOutput(otherOutput);
        if (otherOutput.Amount >= ConfigManager.Instance.DustLimitAmount)
        {
            otherOutput.Index = (uint)AddOutput(otherOutput);
        }
        else
        {
            otherOutput.Amount = LightningMoney.Zero;
        }
    }

    private void SetTxId()
    {
        ToRemoteOutput.TxId = TxId;
        ToLocalOutput.TxId = TxId;

        foreach (var offeredHtlcOutput in _offeredHtlcOutputs)
        {
            offeredHtlcOutput.TxId = TxId;
        }

        foreach (var receivedHtlcOutput in _receivedHtlcOutputs)
        {
            receivedHtlcOutput.TxId = TxId;
        }

        if (ConfigManager.Instance.IsOptionAnchorOutput)
        {
            if (LocalAnchorOutput is not null)
            {
                LocalAnchorOutput.TxId = TxId;
            }

            if (RemoteAnchorOutput is not null)
            {
                RemoteAnchorOutput.TxId = TxId;
            }
        }
    }
}