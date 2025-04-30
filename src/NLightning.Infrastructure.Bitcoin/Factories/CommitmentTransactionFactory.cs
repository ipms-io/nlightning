using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.Transactions;
using Domain.Bitcoin.Services;
using Domain.Money;
using Domain.Node.Options;
using Interfaces;
using Outputs;
using Protocol.Models;
using Transactions;

public class CommitmentTransactionFactory : ICommitmentTransactionFactory
{
    private readonly IFeeService _feeService;
    private readonly NodeOptions _nodeOptions;

    public CommitmentTransactionFactory(IFeeService feeService, IOptions<NodeOptions> nodeOptions)
    {
        _feeService = feeService;
        _nodeOptions = nodeOptions.Value;
    }

    public ITransaction CreateCommitmentTransaction(IOutput output, PubKey localPaymentBasepoint,
                                                    PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                    PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                    LightningMoney toRemoteAmount, uint toSelfDelay,
                                                    CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                    params BitcoinSecret[] secrets)
    {
        if (output is not FundingOutput fundingOutput)
            throw new ArgumentException("Invalid funding output type", nameof(output));

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

    public ITransaction CreateCommitmentTransaction(IOutput output, PubKey localPaymentBasepoint,
                                                    PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                                    PubKey revocationPubKey, LightningMoney toLocalAmount,
                                                    LightningMoney toRemoteAmount, uint toSelfDelay,
                                                    CommitmentNumber commitmentNumber, bool isChannelFunder,
                                                    IEnumerable<IOutput> offeredHtlcs,
                                                    IEnumerable<IOutput> receivedHtlcs,
                                                    params BitcoinSecret[] secrets)
    {
        if (output is not FundingOutput fundingOutput)
            throw new ArgumentException("Invalid funding output type", nameof(output));

        var commitmentTransaction = new CommitmentTransaction(_nodeOptions.AnchorAmount, _nodeOptions.DustLimitAmount,
                                                              _nodeOptions.MustTrimHtlcOutputs, _nodeOptions.Network,
                                                              fundingOutput, localPaymentBasepoint,
                                                              remotePaymentBasepoint, localDelayedPubKey,
                                                              revocationPubKey, toLocalAmount, toRemoteAmount,
                                                              toSelfDelay, commitmentNumber, isChannelFunder);

        foreach (var offeredHtlc in offeredHtlcs)
        {
            if (offeredHtlc is not OfferedHtlcOutput offeredHtlcOutput)
                throw new ArgumentException("Invalid offered HTLC output type", nameof(offeredHtlcs));

            commitmentTransaction.AddOfferedHtlcOutput(offeredHtlcOutput);
        }

        foreach (var receivedHtlc in receivedHtlcs)
        {
            if (receivedHtlc is not ReceivedHtlcOutput receivedHtlcOutput)
                throw new ArgumentException("Invalid offered HTLC output type", nameof(receivedHtlcs));

            commitmentTransaction.AddReceivedHtlcOutput(receivedHtlcOutput);
        }

        commitmentTransaction.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        commitmentTransaction.SignTransaction(secrets);

        return commitmentTransaction;
    }
}