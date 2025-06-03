namespace NLightning.Domain.Transactions.Factories;

using Channels.Enums;
using Channels.Models;
using Money;
using Constants;
using Enums;
using Interfaces;
using Models;
using Outputs;

public class CommitmentTrasanctionModelModelFactory : ICommitmentTransactionModelFactory
{
    public CommitmentTransactionModel CreateCommitmentTransactionModel(Channel channel, CommitmentSide commitmentSide)
    {
        // Create base output information
        ToLocalOutputInfo? toLocalOutput = null;
        ToRemoteOutputInfo? toRemoteOutput = null;
        AnchorOutputInfo? localAnchorOutput = null;
        AnchorOutputInfo? remoteAnchorOutput = null;
        var offeredHtlcOutputs = new List<OfferedHtlcOutputInfo>();
        var receivedHtlcOutputs = new List<ReceivedHtlcOutputInfo>();
        
        // Determine if we're building for a local or a remote commitment
        var commitmentNumber = commitmentSide == CommitmentSide.Local 
            ? channel.LocalCommitmentNumber 
            : channel.RemoteCommitmentNumber;
        var htlcs = commitmentSide == CommitmentSide.Local 
            ? channel.LocalOfferedHtlcs 
            : channel.RemoteOfferedHtlcs;
        
        // Calculate base weight
        var weight = WeightConstants.TransactionBaseWeight
                     + TransactionConstants.CommitmentTransactionInputWeight
                     + (htlcs?.Count ?? 0) * WeightConstants.HtlcOutputWeight;
        
        // Set initial amounts for to_local and to_remote outputs
        var toLocalAmount = commitmentSide == CommitmentSide.Local
            ? channel.LocalBalance
            : channel.RemoteBalance;
        
        var toRemoteAmount = commitmentSide == CommitmentSide.Local
            ? channel.RemoteBalance
            : channel.LocalBalance;
        
        var localDustLimitAmount = commitmentSide == CommitmentSide.Local
            ? channel.ChannelConfig.LocalDustLimitAmount
            : channel.ChannelConfig.RemoteDustLimitAmount;
        
        var remoteDustLimitAmount = commitmentSide == CommitmentSide.Local
            ? channel.ChannelConfig.RemoteDustLimitAmount
            : channel.ChannelConfig.LocalDustLimitAmount;
        
        if (htlcs is { Count: > 0 })
        {
            // Calculate htlc weight and fee
            var offeredHtlcWeight = channel.ChannelConfig.OptionAnchorOutputs
                ? WeightConstants.HtlcTimeoutWeightAnchors
                : WeightConstants.HtlcTimeoutWeightNoAnchors;
            var offeredHtlcFee = offeredHtlcWeight * channel.ChannelConfig.FeeRateAmountPerKw.Satoshi / 1000L;

            var receivedHtlcWeight = channel.ChannelConfig.OptionAnchorOutputs
                ? WeightConstants.HtlcSuccessWeightAnchors
                : WeightConstants.HtlcSuccessWeightNoAnchors;
            var receivedHtlcFee = receivedHtlcWeight * channel.ChannelConfig.FeeRateAmountPerKw.Satoshi / 1000L;
            
            // Select keys according to the commitment side
            var localKeys = commitmentSide == CommitmentSide.Local ? channel.LocalKeySet : channel.RemoteKeySet;
            var remoteKeys = commitmentSide == CommitmentSide.Local ? channel.RemoteKeySet : channel.LocalKeySet;

            foreach (var htlc in htlcs)
            {
                // Determine if this is an offered or received HTLC from the perspective of the commitment holder
                var isOffered = commitmentSide == CommitmentSide.Local
                    ? htlc.Direction == HtlcDirection.Outgoing
                    : htlc.Direction == HtlcDirection.Incoming;
                
                // Calculate the amounts after subtracting fees
                var htlcFee = isOffered ? offeredHtlcFee : receivedHtlcFee;
                var htlcAmount = htlc.Amount - htlcFee;
                
                // Offered or received depends on isOffered plus dust check
                if (isOffered)
                {
                    if (htlcAmount >= localDustLimitAmount)
                    {
                        weight += WeightConstants.HtlcOutputWeight;
                        offeredHtlcOutputs.Add(new OfferedHtlcOutputInfo(
                            htlc,
                            remoteKeys.RevocationCompactBasepoint,
                            localKeys.HtlcCompactBasepoint,
                            remoteKeys.HtlcCompactBasepoint));
                    }
                }
                else
                {
                    if (htlcAmount >= remoteDustLimitAmount)
                    {
                        weight += WeightConstants.HtlcOutputWeight;
                        receivedHtlcOutputs.Add(new ReceivedHtlcOutputInfo(
                            htlc,
                            remoteKeys.RevocationCompactBasepoint,
                            localKeys.HtlcCompactBasepoint,
                            remoteKeys.HtlcCompactBasepoint));
                    }
                }
            }
        }
        
        LightningMoney fee;
        // Create anchor outputs if option_anchors is negotiated
        if (channel.ChannelConfig.OptionAnchorOutputs)
        {
            localAnchorOutput = new AnchorOutputInfo(channel.LocalKeySet.FundingCompactPubKey, true);
            remoteAnchorOutput = new AnchorOutputInfo(channel.RemoteKeySet.FundingCompactPubKey, false);

            weight += WeightConstants.AnchorOutputWeight * 2;
            fee = weight * channel.ChannelConfig.FeeRateAmountPerKw.Satoshi / 1000L;

            ref var feePayerAmount = ref GetFeePayerAmount(commitmentSide, channel.IsInitiator, ref toLocalAmount, ref toRemoteAmount);
            AdjustForAnchorOutputs(ref feePayerAmount, fee, TransactionConstants.AnchorOutputAmount);
        }
        else
        {
            fee = weight * channel.ChannelConfig.FeeRateAmountPerKw.Satoshi / 1000L;

            ref var feePayerAmount = ref GetFeePayerAmount(commitmentSide, channel.IsInitiator, ref toLocalAmount, ref toRemoteAmount);

            // Simple fee deduction when no anchors
            if (feePayerAmount > fee)
                feePayerAmount -= fee;
            else
                feePayerAmount = LightningMoney.Zero;
        }
        
        // Select keysets for to_local and to_remote based on the commitment side
        var toLocalRevocationBasepoint = commitmentSide == CommitmentSide.Local
            ? channel.RemoteKeySet.RevocationCompactBasepoint
            : channel.LocalKeySet.RevocationCompactBasepoint;
        var toLocalDelayedPaymentBasepoint = commitmentSide == CommitmentSide.Local
            ? channel.LocalKeySet.DelayedPaymentCompactBasepoint
            : channel.RemoteKeySet.DelayedPaymentCompactBasepoint;

        var toRemotePaymentBasepoint = commitmentSide == CommitmentSide.Local
            ? channel.RemoteKeySet.PaymentCompactBasepoint
            : channel.LocalKeySet.PaymentCompactBasepoint;

        // Only create output if the amount is above the dust limit
        if (toLocalAmount >= localDustLimitAmount)
        {
            toLocalOutput = new ToLocalOutputInfo(
                toLocalAmount,
                toLocalRevocationBasepoint,
                toLocalDelayedPaymentBasepoint,
                channel.ChannelConfig.ToSelfDelay);
        }

        if (toRemoteAmount >= remoteDustLimitAmount)
        {
            toRemoteOutput = new ToRemoteOutputInfo(
                toRemoteAmount,
                toRemotePaymentBasepoint,
                channel.ChannelConfig.OptionAnchorOutputs);
        }
        
        if (offeredHtlcOutputs.Count == 0 && receivedHtlcOutputs.Count == 0)
        {
            // If no HTLCs and no to_local, we can remove our anchor output
            if (toLocalOutput is null)
                localAnchorOutput = null;
            
            // If no HTLCs and no to_remote, we can remove their anchor output
            if (toRemoteOutput is null)
                remoteAnchorOutput = null;
        }
        
        // Create and return the commitment transaction model
        return new CommitmentTransactionModel(commitmentNumber, fee, channel.FundingOutput, localAnchorOutput,
                                              remoteAnchorOutput, toLocalOutput, toRemoteOutput,offeredHtlcOutputs,
                                              receivedHtlcOutputs);
    }
    
    private ref LightningMoney GetFeePayerAmount(CommitmentSide side, bool isInitiator, ref LightningMoney toLocal,
                                                 ref LightningMoney toRemote)
    {
        // If we're the initiator, and it's our tx, deduct from toLocal
        // If not initiator and our tx, deduct from toRemote
        // For remote tx, logic is reversed
        if ((side == CommitmentSide.Local && isInitiator) || (side == CommitmentSide.Remote && !isInitiator))
            return ref toLocal;

        return ref toRemote;
    }

    private void AdjustForAnchorOutputs(ref LightningMoney amount, LightningMoney fee, LightningMoney anchorAmount)
    {
        if (amount > fee)
        {
            amount -= fee;
            if (amount > anchorAmount)
                amount -= (amount - anchorAmount - fee); // Adjust as per your logic
            else
                amount = LightningMoney.Zero;
        }
        else
            amount = LightningMoney.Zero;
    }
}