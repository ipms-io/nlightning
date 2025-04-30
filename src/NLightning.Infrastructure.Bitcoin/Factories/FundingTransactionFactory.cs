using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Bitcoin.Factories;
using Domain.Bitcoin.Services;
using Domain.Bitcoin.Transactions;
using Domain.Money;
using Domain.Node.Options;
using Transactions;
using Network = Domain.ValueObjects.Network;

public class FundingTransactionFactory : IFundingTransactionFactory
{
    private readonly IFeeService _feeService;
    private readonly NodeOptions _nodeOptions;
    private readonly Network _network;

    public FundingTransactionFactory(IFeeService feeService, IOptions<NodeOptions> nodeOptions)
    {
        _feeService = feeService;
        _nodeOptions = nodeOptions.Value;
        _network = _nodeOptions.Network;
    }

    public ITransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                 LightningMoney fundingSatoshis, Script changeScript,
                                                 Coin[] coins, params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(_nodeOptions.DustLimitAmount, _nodeOptions.HasAnchorOutputs,
                                               _network, localFundingPubKey, remoteFundingPubKey,
                                               fundingSatoshis, changeScript, coins);

        fundingTx.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        fundingTx.SignTransaction(secrets);

        return fundingTx;
    }

    public ITransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                 LightningMoney fundingSatoshis, Script redeemScript,
                                                 Script changeScript, Coin[] coins,
                                                 params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(_nodeOptions.DustLimitAmount, _nodeOptions.HasAnchorOutputs,
                                               _network, localFundingPubKey, remoteFundingPubKey,
                                               fundingSatoshis, redeemScript, changeScript, coins);

        fundingTx.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        fundingTx.SignTransaction(secrets);

        return fundingTx;
    }
}