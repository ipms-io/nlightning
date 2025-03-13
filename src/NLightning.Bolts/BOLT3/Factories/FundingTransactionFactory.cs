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

    public Transaction CreateFundingTransactionAsync(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                            ulong fundingSatoshis, Script changeScript,
                                                            Coin[] coins,
                                                            params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, changeScript,
                                               coins);

        return fundingTx.GetSignedTransaction(_feeCalculator, secrets);
    }
}