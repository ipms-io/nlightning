using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Infrastructure.Bitcoin.Transactions;

using Domain.Money;
using Domain.Protocol.Constants;
using Domain.Protocol.Signers;
using Domain.ValueObjects;
using Outputs;
using Protocol.Models;

/// <summary>
/// Represents a commitment transaction.
/// </summary>
public class CommitmentTransaction : BaseTransaction
{
    #region Private Fields
    private readonly LightningMoney _anchorAmount;
    private readonly LightningMoney _dustLimitAmount;
    private readonly bool _isChannelFunder;
    private readonly bool _mustTrimHtlcOutputs;

    private LightningMoney _toFunderAmount;
    #endregion

    #region Public Properties
    public ToLocalOutput ToLocalOutput { get; }
    public ToRemoteOutput ToRemoteOutput { get; }
    public ToAnchorOutput? LocalAnchorOutput { get; private set; }
    public ToAnchorOutput? RemoteAnchorOutput { get; private set; }
    public CommitmentNumber CommitmentNumber { get; }
    public IList<OfferedHtlcOutput> OfferedHtlcOutputs { get; } = [];
    public IList<ReceivedHtlcOutput> ReceivedHtlcOutputs { get; } = [];

    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="CommitmentTransaction"/> class.
    /// </summary>
    /// <param name="anchorAmount">The anchor amount.</param>
    /// <param name="network">The network type.</param>
    /// <param name="mustTrimHtlcOutputs">Indicates if HTLC outputs must be trimmed.</param>
    /// <param name="dustLimitAmount"></param>
    /// <param name="fundingOutput">The funding coin.</param>
    /// <param name="localPaymentBasepoint">The local public key.</param>
    /// <param name="remotePaymentBasepoint">The remote public key.</param>
    /// <param name="localDelayedPubKey">The local delayed public key.</param>
    /// <param name="remoteRevocationPubKey">The revocation public key.</param>
    /// <param name="toLocalAmount">The amount for the to_local output in satoshis.</param>
    /// <param name="toRemoteAmount">The amount for the to_remote output in satoshis.</param>
    /// <param name="toSelfDelay">The to_self_delay in blocks.</param>
    /// <param name="commitmentNumber">The commitment number object.</param>
    /// <param name="isChannelFunder">Indicates if the local node is the channel funder.</param>
    internal CommitmentTransaction(LightningMoney anchorAmount, LightningMoney dustLimitAmount,
                                   bool mustTrimHtlcOutputs, Network network, FundingOutput fundingOutput,
                                   PubKey localPaymentBasepoint, PubKey remotePaymentBasepoint,
                                   PubKey localDelayedPubKey, PubKey remoteRevocationPubKey,
                                   LightningMoney toLocalAmount, LightningMoney toRemoteAmount, uint toSelfDelay,
                                   CommitmentNumber commitmentNumber, bool isChannelFunder)
        : base(!anchorAmount.IsZero, network, TransactionConstants.COMMITMENT_TRANSACTION_VERSION, SigHash.All,
               (fundingOutput.ToCoin(), commitmentNumber.CalculateSequence()))
    {
        ArgumentNullException.ThrowIfNull(localPaymentBasepoint);
        ArgumentNullException.ThrowIfNull(remotePaymentBasepoint);
        ArgumentNullException.ThrowIfNull(localDelayedPubKey);
        ArgumentNullException.ThrowIfNull(remoteRevocationPubKey);

        if (toLocalAmount.IsZero && toRemoteAmount.IsZero)
        {
            throw new ArgumentException("Both toLocalAmount and toRemoteAmount cannot be zero.");
        }

        _anchorAmount = anchorAmount;
        _dustLimitAmount = dustLimitAmount;
        _isChannelFunder = isChannelFunder;
        _mustTrimHtlcOutputs = mustTrimHtlcOutputs;
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
        ToLocalOutput = new ToLocalOutput(localDelayedPubKey, remoteRevocationPubKey, toSelfDelay, localAmount);
        AddOutput(ToLocalOutput);

        // to_remote output
        ToRemoteOutput = new ToRemoteOutput(!anchorAmount.IsZero, remotePaymentBasepoint, remoteAmount);
        AddOutput(ToRemoteOutput);

        if (anchorAmount == LightningMoney.Zero)
        {
            return;
        }

        // Local anchor output
        LocalAnchorOutput = new ToAnchorOutput(fundingOutput.LocalPubKey, anchorAmount);
        AddOutput(LocalAnchorOutput);

        // Remote anchor output
        RemoteAnchorOutput = new ToAnchorOutput(fundingOutput.RemotePubKey, anchorAmount);
        AddOutput(RemoteAnchorOutput);
    }
    #endregion

    #region Public Methods
    public void AddOfferedHtlcOutput(OfferedHtlcOutput offeredHtlcOutput)
    {
        if (Finalized)
        {
            throw new InvalidOperationException("You can't add outputs to an already finalized transaction.");
        }

        // Add output
        OfferedHtlcOutputs.Add(offeredHtlcOutput);
        AddOutput(offeredHtlcOutput);
    }

    public void AddReceivedHtlcOutput(ReceivedHtlcOutput receivedHtlcOutput)
    {
        if (Finalized)
        {
            throw new InvalidOperationException("You can't add outputs to an already finalized transaction.");
        }

        ReceivedHtlcOutputs.Add(receivedHtlcOutput);
        AddOutput(receivedHtlcOutput);
    }

    public List<ECDSASignature> AppendRemoteSignatureAndSign(ILightningSigner signer, ECDSASignature remoteSignature,
                                                             PubKey remotePubKey)
    {
        AppendRemoteSignatureToTransaction(signer, new TransactionSignature(remoteSignature), remotePubKey);
        return SignTransactionWithExistingKeys(signer);
    }

    public Transaction GetSignedTransaction()
    {
        if (Finalized)
        {
            return FinalizedTransaction;
        }

        throw new InvalidOperationException("You have to sign and finalize the transaction first.");
    }

    public void ReplaceFundingOutput(FundingOutput oldFundingOutput, FundingOutput newFundingOutput)
    {
        RemoveCoin(oldFundingOutput.ToCoin());
        AddCoin(newFundingOutput.ToCoin(), CommitmentNumber.CalculateSequence());
    }
    #endregion

    #region Internal Methods
    internal override void ConstructTransaction(LightningMoney currentFeePerKw)
    {
        // Calculate base fee
        var outputWeight = CalculateOutputWeight();
        var calculatedFee = (outputWeight + TransactionConstants.COMMITMENT_TRANSACTION_INPUT_WEIGHT)
                          * currentFeePerKw.Satoshi / 1000L;
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
        if (!_anchorAmount.IsZero && !_toFunderAmount.IsZero)
        {
            _toFunderAmount -= _anchorAmount;
            _toFunderAmount -= _anchorAmount;
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
        if (_mustTrimHtlcOutputs)
        {
            var offeredHtlcWeight = _anchorAmount.IsZero
                ? WeightConstants.HTLC_TIMEOUT_WEIGHT_NO_ANCHORS
                : WeightConstants.HTLC_TIMEOUT_WEIGHT_ANCHORS;
            var offeredHtlcFee = offeredHtlcWeight * currentFeePerKw.Satoshi / 1000L;
            foreach (var offeredHtlcOutput in OfferedHtlcOutputs)
            {
                var htlcAmount = offeredHtlcOutput.Amount - offeredHtlcFee;
                if (htlcAmount < _dustLimitAmount)
                {
                    RemoveOutput(offeredHtlcOutput);
                }
            }

            var receivedHtlcWeight = _anchorAmount.IsZero
                ? WeightConstants.HTLC_SUCCESS_WEIGHT_NO_ANCHORS
                : WeightConstants.HTLC_SUCCESS_WEIGHT_ANCHORS;
            var receivedHtlcFee = receivedHtlcWeight * currentFeePerKw.Satoshi / 1000L;
            foreach (var receivedHtlcOutput in ReceivedHtlcOutputs)
            {
                var htlcAmount = receivedHtlcOutput.Amount - receivedHtlcFee;
                if (htlcAmount < _dustLimitAmount)
                {
                    RemoveOutput(receivedHtlcOutput);
                }
            }
        }

        // Anchors are always needed, except when one of the outputs is zero and there's no htlc output
        if (!_anchorAmount.IsZero && !Outputs.Any(o => o is BaseHtlcOutput))
        {
            if (ToLocalOutput.Amount.IsZero)
            {
                RemoveOutput(LocalAnchorOutput);
            }

            if (ToRemoteOutput.Amount.IsZero)
            {
                RemoveOutput(RemoteAnchorOutput);
            }
        }

        // Order Outputs
        AddOrderedOutputsToTransaction();
    }

    internal new List<ECDSASignature> SignTransaction(ILightningSigner signer, params BitcoinSecret[] secrets)
    {
        var signatures = base.SignTransaction(signer, secrets);

        SetTxIdAndIndexes();
        
        return signatures;
    }
    #endregion

    #region Private Methods
    private void SetLocalAndRemoteAmounts(BaseOutput funderOutput, BaseOutput otherOutput)
    {
        if (_toFunderAmount >= _dustLimitAmount)
        {
            if (_toFunderAmount != funderOutput.Amount)
            {
                // Remove old output
                RemoveOutput(funderOutput);

                // Set amount
                funderOutput.Amount = _toFunderAmount;

                // Add new output
                AddOutput(funderOutput);
            }
        }
        else
        {
            RemoveOutput(funderOutput);
            funderOutput.Amount = LightningMoney.Zero;
        }

        RemoveOutput(otherOutput);
        if (otherOutput.Amount >= _dustLimitAmount)
        {
            AddOutput(otherOutput);
        }
        else
        {
            otherOutput.Amount = LightningMoney.Zero;
        }
    }

    private void SetTxIdAndIndexes()
    {
        ToRemoteOutput.TxId = TxId;
        ToRemoteOutput.Index = Outputs.IndexOf(ToRemoteOutput);

        ToLocalOutput.TxId = TxId;
        ToRemoteOutput.Index = Outputs.IndexOf(ToLocalOutput);

        foreach (var offeredHtlcOutput in OfferedHtlcOutputs)
        {
            offeredHtlcOutput.TxId = TxId;
            offeredHtlcOutput.Index = Outputs.IndexOf(offeredHtlcOutput);
        }

        foreach (var receivedHtlcOutput in ReceivedHtlcOutputs)
        {
            receivedHtlcOutput.TxId = TxId;
            receivedHtlcOutput.Index = Outputs.IndexOf(receivedHtlcOutput);
        }

        if (!_anchorAmount.IsZero)
        {
            if (LocalAnchorOutput is not null)
            {
                LocalAnchorOutput.TxId = TxId;
                LocalAnchorOutput.Index = Outputs.IndexOf(LocalAnchorOutput);
            }

            if (RemoteAnchorOutput is not null)
            {
                RemoteAnchorOutput.TxId = TxId;
                RemoteAnchorOutput.Index = Outputs.IndexOf(RemoteAnchorOutput);
            }
        }
    }
    #endregion
}