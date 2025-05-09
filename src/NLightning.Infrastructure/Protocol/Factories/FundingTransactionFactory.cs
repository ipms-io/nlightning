using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Protocol.Factories;

using Application.Interfaces.Services;
using Application.Options;
using Domain.Protocol.Interfaces;
using Domain.ValueObjects;
using Transactions;

public class FundingTransactionFactory : IFundingTransactionFactory
{
    private readonly IFeeService _feeService;
    private readonly NodeOptions _nodeOptions;

    public FundingTransactionFactory(IFeeService feeService, IOptions<NodeOptions> nodeOptions)
    {
        _feeService = feeService;
        _nodeOptions = nodeOptions.Value;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                       LightningMoney fundingSatoshis, Script changeScript,
                                                       Coin[] coins, params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(_nodeOptions.DustLimitAmount, _nodeOptions.HasAnchorOutputs,
                                               _nodeOptions.Network, localFundingPubKey, remoteFundingPubKey,
                                               fundingSatoshis, changeScript, coins);

        fundingTx.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        fundingTx.SignTransaction(secrets);

        return fundingTx;
    }

    public FundingTransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                                       LightningMoney fundingSatoshis, Script redeemScript,
                                                       Script changeScript, Coin[] coins,
                                                       params BitcoinSecret[] secrets)
    {
        var fundingTx = new FundingTransaction(_nodeOptions.DustLimitAmount, _nodeOptions.HasAnchorOutputs,
                                               _nodeOptions.Network, localFundingPubKey, remoteFundingPubKey,
                                               fundingSatoshis, redeemScript, changeScript, coins);

        fundingTx.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        fundingTx.SignTransaction(secrets);

        return fundingTx;
    }
}