using NBitcoin;

namespace NLightning.Bolts.BOLT3.Factories;

using Calculators;
using Transactions;

public class FundingTransactionFactory
{
    private readonly FeeCalculator _feeCalculator;

    public FundingTransactionFactory(FeeCalculator feeCalculator)
    {
        _feeCalculator = feeCalculator;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                LightningMoney fundingSatoshis, Script changeScript, Coin[] coins,
                                                params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, changeScript,
                                               coins);

        fundingTx.SignTransaction(_feeCalculator, secrets);

        return fundingTx;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                       LightningMoney fundingSatoshis, Script redeemScript,
                                                       Script changeScript, Coin[] coins, params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, redeemScript,
                                               changeScript, coins);

        fundingTx.SignTransaction(_feeCalculator, secrets);

        return fundingTx;
    }
}