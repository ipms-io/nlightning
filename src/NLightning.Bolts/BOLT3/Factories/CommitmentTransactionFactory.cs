using NBitcoin;

namespace NLightning.Bolts.BOLT3.Factories;

using Calculators;
using Transactions;
using Types;

public class CommitmentTransactionFactory
{
    private readonly FeeCalculator _feeCalculator;

    public CommitmentTransactionFactory(FeeCalculator feeCalculator)
    {
        _feeCalculator = feeCalculator;
    }

    public CommitmentTransaction CreateCommitmentTransaction(Coin fundingCoin, PubKey localPaymentBasepoint,
                                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                             params BitcoinSecret[] secrets)
    {
        // var fundingScriptPubKey = 

        var commitmentTransaction = new CommitmentTransaction(fundingCoin, localPaymentBasepoint, remotePaymentBasepoint,
                                                              localDelayedPubKey, revocationPubKey, toLocalAmount,
                                                              toRemoteAmount, toSelfDelay, commitmentNumber,
                                                              isChannelFunder);

        commitmentTransaction.SignTransaction(_feeCalculator, secrets);

        return commitmentTransaction;
    }
}