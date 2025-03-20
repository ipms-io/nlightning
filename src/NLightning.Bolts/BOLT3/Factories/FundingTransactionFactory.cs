using NBitcoin;

namespace NLightning.Bolts.BOLT3.Factories;

using Common.Interfaces;
using Transactions;

public class FundingTransactionFactory
{
    private readonly IFeeService _feeService;

    public FundingTransactionFactory(IFeeService feeService)
    {
        _feeService = feeService;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                       LightningMoney fundingSatoshis, Script changeScript, Coin[] coins,
                                                       params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, changeScript,
                                               coins);

        fundingTx.SignTransaction(_feeService, secrets);

        return fundingTx;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                       LightningMoney fundingSatoshis, Script redeemScript,
                                                       Script changeScript, Coin[] coins, params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(localFundingPubKey, remoteFundingPubKey, fundingSatoshis, redeemScript,
                                               changeScript, coins);

        fundingTx.SignTransaction(_feeService, secrets);

        return fundingTx;
    }
}