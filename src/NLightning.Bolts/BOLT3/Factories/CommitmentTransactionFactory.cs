using NBitcoin;

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

    public CommitmentTransaction CreateCommitmentTransaction(Coin fundingCoin, PubKey localPaymentBasepoint,
                                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                             params BitcoinSecret[] secrets)
    {
        var commitmentTransaction = new CommitmentTransaction(fundingCoin, localPaymentBasepoint, remotePaymentBasepoint,
                                                              localDelayedPubKey, revocationPubKey, toLocalAmount,
                                                              toRemoteAmount, toSelfDelay, commitmentNumber,
                                                              isChannelFunder);

        commitmentTransaction.SignTransaction(_feeService, secrets);

        return commitmentTransaction;
    }
}