using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.Transactions.Factories;
using NLightning.Domain.Bitcoin.Transactions.Outputs;
using NLightning.Tests.Utils.Mocks;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Domain.Tests.Transactions.Factories;

using Domain.Bitcoin.Interfaces;
using Domain.Channels.Enums;
using Domain.Channels.Models;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Money;
using Domain.Protocol.Interfaces;
using Domain.Protocol.ValueObjects;
using Enums;

public class CommitmentTransactionModelFactoryTests
{
    [Fact]
    public void Given_ValidParameters_When_Creating_Then_ReturnsCommitmentTransactionModel()
    {
        // Given
        var emptyCompactPubKey = new CompactPubKey([
            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00
        ]);
        var sha256Mock = new Mock<FakeSha256>();
        sha256Mock.Setup(x => x.GetHashAndReset())
                  .Returns(Convert.FromHexString("C8BFEA84214B45899482A4BAD1D85C42130743ED78BA3711F5532BB038521914"));

        var commitmentKeyDerivationService = new Mock<ICommitmentKeyDerivationService>();
        var lightningSigner = new Mock<ILightningSigner>();

        var channelConfig = new ChannelConfig(LightningMoney.Zero, LightningMoney.Satoshis(15_000), LightningMoney.Zero,
                                              LightningMoney.Zero, 0, LightningMoney.Zero, 0, false,
                                              LightningMoney.Zero, Bolt3AppendixCVectors.LocalDelay, FeatureSupport.No);
        var fundingOutputInfo = new FundingOutputInfo(Bolt3AppendixBVectors.FundingSatoshis,
                                                      Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes(),
                                                      Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes())
        {
            TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
            Index = 0,
        };
        var commitmentNumber = new CommitmentNumber(Bolt3AppendixCVectors.NodeAPaymentBasepoint.ToBytes(),
                                                    Bolt3AppendixCVectors.NodeBPaymentBasepoint.ToBytes(),
                                                    sha256Mock.Object,
                                                    Bolt3AppendixCVectors.CommitmentNumber);
        var localKeySet = new ChannelKeySetModel(0, Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes(),
                                                 Bolt3AppendixCVectors.NodeARevocationPubkey.ToBytes(),
                                                 Bolt3AppendixCVectors.NodeAPaymentBasepoint.ToBytes(),
                                                 Bolt3AppendixCVectors.NodeADelayedPubkey.ToBytes(),
                                                 emptyCompactPubKey, emptyCompactPubKey);
        var remoteKeySet = new ChannelKeySetModel(0, Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes(),
                                                  emptyCompactPubKey,
                                                  Bolt3AppendixCVectors.NodeBPaymentBasepoint.ToBytes(),
                                                  emptyCompactPubKey, emptyCompactPubKey, emptyCompactPubKey);
        var channel = new ChannelModel(channelConfig, ChannelId.Zero, commitmentNumber, fundingOutputInfo, true, null,
                                       null, Bolt3AppendixCVectors.Tx0ToLocalMsat, localKeySet, 1, 0,
                                       Bolt3AppendixCVectors.ToRemoteMsat, remoteKeySet, 1, emptyCompactPubKey, 0,
                                       ChannelState.V1Opening, ChannelVersion.V1);

        var factory =
            new CommitmentTransactionModelFactory(commitmentKeyDerivationService.Object, lightningSigner.Object);

        // When
        var transactionModel = factory.CreateCommitmentTransactionModel(channel, CommitmentSide.Local);

        // Then
        Assert.NotNull(transactionModel);
        Assert.NotNull(transactionModel.FundingOutput);
        Assert.NotNull(transactionModel.ToLocalOutput);
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx0ToLocalAmount, transactionModel.ToLocalOutput.Amount);
        Assert.NotNull(transactionModel.ToRemoteOutput);
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx0ToRemoteAmount, transactionModel.ToRemoteOutput.Amount);
    }
}