using NBitcoin;
using NLightning.Common.Managers;

namespace NLightning.Bolts.BOLT3.Factories;

using Transactions;
using Calculators;

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
                                                            params Key[] keys)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, changeScript,
                                               coins);
                
        return fundingTx.SignAndFinalizeTransaction(_feeCalculator, keys);
    }
}