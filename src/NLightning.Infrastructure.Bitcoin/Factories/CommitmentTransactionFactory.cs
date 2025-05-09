using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.Transactions;
using Domain.Bitcoin.Services;
using Domain.Money;
using Domain.Node.Options;
using Domain.Protocol.Signers;
using Interfaces;
using Outputs;
using Protocol.Models;
using Transactions;

public class CommitmentTransactionFactory : ICommitmentTransactionFactory
{
    private readonly IFeeService _feeService;
    private readonly ILightningSigner _lightningSigner;

    public CommitmentTransactionFactory(IFeeService feeService, ILightningSigner lightningSigner)
    {
        _feeService = feeService;
        _lightningSigner = lightningSigner;
    }

    public ITransaction CreateCommitmentTransaction(NodeOptions nodeOptions, IOutput output,
                                                    PubKey localPaymentBasepoint, PubKey remotePaymentBasepoint,
                                                    PubKey localDelayedPubKey, PubKey revocationPubKey,
                                                    LightningMoney toLocalAmount, LightningMoney toRemoteAmount,
                                                    CommitmentNumber commitmentNumber, bool isFunder,
                                                    params BitcoinSecret[] secrets)
    {
        if (output is not FundingOutput fundingOutput)
            throw new ArgumentException("Invalid funding output type", nameof(output));

        var commitmentTransaction = new CommitmentTransaction(nodeOptions.AnchorAmount, nodeOptions.DustLimitAmount,
                                                              nodeOptions.MustTrimHtlcOutputs, nodeOptions.Network,
                                                              fundingOutput, localPaymentBasepoint,
                                                              remotePaymentBasepoint, localDelayedPubKey,
                                                              revocationPubKey, toLocalAmount, toRemoteAmount,
                                                              nodeOptions.ToSelfDelay, commitmentNumber, isFunder);

        commitmentTransaction.ConstructTransaction(_feeService.GetCachedFeeRatePerKw());

        commitmentTransaction.SignTransaction(_lightningSigner, secrets);

        return commitmentTransaction;
    }

    public ITransaction CreateCommitmentTransaction(NodeOptions nodeOptions, IOutput output,
                                                    PubKey localPaymentBasepoint, PubKey remotePaymentBasepoint,
                                                    PubKey localDelayedPubKey, PubKey revocationPubKey,
                                                    LightningMoney toLocalAmount, LightningMoney toRemoteAmount,
                                                    CommitmentNumber commitmentNumber,
                                                    IEnumerable<IOutput> offeredHtlcs,
                                                    IEnumerable<IOutput> receivedHtlcs, bool isFunder,
                                                    params BitcoinSecret[] secrets)
    {
        if (output is not FundingOutput fundingOutput)
            throw new ArgumentException("Invalid funding output type", nameof(output));

        var commitmentTransaction = new CommitmentTransaction(nodeOptions.AnchorAmount, nodeOptions.DustLimitAmount,
                                                              nodeOptions.MustTrimHtlcOutputs, nodeOptions.Network,
                                                              fundingOutput, localPaymentBasepoint,
                                                              remotePaymentBasepoint, localDelayedPubKey,
                                                              revocationPubKey, toLocalAmount, toRemoteAmount,
                                                              nodeOptions.ToSelfDelay, commitmentNumber, isFunder);

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

        commitmentTransaction.SignTransaction(_lightningSigner, secrets);

        return commitmentTransaction;
    }
}