using NBitcoin;
using NLightning.Bolts.BOLT3.Outputs;

namespace NLightning.Bolts.BOLT3.Factories;

using Common.Interfaces;
using Transactions;
using Types;

public class CommitmentTransactionFactory
{
    private readonly IFeeService _feeService;

    public CommitmentTransactionFactory(IFeeService feeService)
    {
        _feeService = feeService;
    }

    public CommitmentTransaction CreateCommitmentTransaction(FundingOutput fundingOutput, PubKey localPaymentBasepoint,
                                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                             params BitcoinSecret[] secrets)
    {
        var commitmentTransaction = new CommitmentTransaction(fundingOutput, localPaymentBasepoint,
                                                              remotePaymentBasepoint, localDelayedPubKey,
                                                              revocationPubKey, toLocalAmount, toRemoteAmount,
                                                              toSelfDelay, commitmentNumber, isChannelFunder);

        commitmentTransaction.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        commitmentTransaction.SignTransaction(secrets);

        return commitmentTransaction;
    }

    public CommitmentTransaction CreateCommitmentTransaction(FundingOutput fundingOutput, PubKey localPaymentBasepoint,
                                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                             IEnumerable<OfferedHtlcOutput> offeredHtlcs,
                                                             IEnumerable<ReceivedHtlcOutput> receivedHtlcs,
                                                             params BitcoinSecret[] secrets)
    {
        var commitmentTransaction = new CommitmentTransaction(fundingOutput, localPaymentBasepoint,
                                                              remotePaymentBasepoint, localDelayedPubKey,
                                                              revocationPubKey, toLocalAmount, toRemoteAmount,
                                                              toSelfDelay, commitmentNumber, isChannelFunder);

        foreach (var offeredHtlc in offeredHtlcs)
        {
            commitmentTransaction.AddOfferedHtlcOutput(offeredHtlc);
        }

        foreach (var receivedHtlc in receivedHtlcs)
        {
            commitmentTransaction.AddReceivedHtlcOutput(receivedHtlc);
        }

        commitmentTransaction.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        commitmentTransaction.SignTransaction(secrets);

        return commitmentTransaction;
    }
}