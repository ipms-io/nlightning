using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Application.Interfaces.Services;
using NLightning.Application.Options;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.ValueObjects;
using NLightning.Infrastructure.Protocol.Models;
using NLightning.Infrastructure.Protocol.Outputs;
using NLightning.Infrastructure.Protocol.Transactions;

namespace NLightning.Infrastructure.Protocol.Factories;

public class CommitmentTransactionFactory : ICommitmentTransactionFactory
{
    private readonly IFeeService _feeService;
    private readonly NodeOptions _nodeOptions;

    public CommitmentTransactionFactory(IFeeService feeService, IOptions<NodeOptions> nodeOptions)
    {
        _feeService = feeService;
        _nodeOptions = nodeOptions.Value;
    }

    public CommitmentTransaction CreateCommitmentTransaction(FundingOutput fundingOutput, PubKey localPaymentBasepoint,
                                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                             params BitcoinSecret[] secrets)
    {
        var commitmentTransaction = new CommitmentTransaction(_nodeOptions.AnchorAmount, _nodeOptions.DustLimitAmount,
                                                              _nodeOptions.MustTrimHtlcOutputs, _nodeOptions.Network,
                                                              fundingOutput, localPaymentBasepoint,
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
        var commitmentTransaction = new CommitmentTransaction(_nodeOptions.AnchorAmount, _nodeOptions.DustLimitAmount,
                                                              _nodeOptions.MustTrimHtlcOutputs, _nodeOptions.Network,
                                                              fundingOutput, localPaymentBasepoint,
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