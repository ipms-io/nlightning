using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Exceptions;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Infrastructure.Bitcoin.Tests.Signers;

using Domain.Bitcoin.ValueObjects;
using Domain.Node.Options;
using Domain.Protocol.Interfaces;
using Domain.Transactions.Outputs;
using Infrastructure.Bitcoin.Builders;
using Infrastructure.Bitcoin.Outputs;
using Infrastructure.Bitcoin.Signers;

public class LocalLightningSignerTests
{
    [Fact]
    public void Given_ValidParameters_When_ValidatingSignature_Then_ReturnsTrue()
    {
        // Given
        var fundingOutputBuilderMock = new Mock<IFundingOutputBuilder>();
        fundingOutputBuilderMock.Setup(x => x.Build(It.IsAny<FundingOutputInfo>()))
                                .Returns(
                                     new FundingOutput(Bolt3AppendixBVectors.FundingSatoshis,
                                                       new PubKey(Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes()),
                                                       new PubKey(Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes()))
                                     {
                                         TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
                                         Index = 0,
                                     });
        var keyDerivationServiceMock = new Mock<IKeyDerivationService>();
        var loggerMock = new Mock<ILogger<LocalLightningSigner>>();
        var nodeOptions = new NodeOptions();
        var secureKeyManagerMock = new Mock<ISecureKeyManager>();
        var testChannelId = ChannelId.Zero;
        var channelSigningInfo = new ChannelSigningInfo(Bolt3AppendixBVectors.ExpectedTxId.ToBytes(), 0,
                                                        Bolt3AppendixBVectors.FundingSatoshis,
                                                        Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes(),
                                                        Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes(), 0);

        var localSigner = new LocalLightningSigner(fundingOutputBuilderMock.Object, keyDerivationServiceMock.Object,
                                                   loggerMock.Object, nodeOptions, secureKeyManagerMock.Object);
        localSigner.RegisterChannel(testChannelId, channelSigningInfo);

        var tx = Bolt3AppendixCVectors.ExpectedCommitTx0;
        tx.Inputs[0].WitScript = null;
        var signedTx = new SignedTransaction(tx.GetHash().ToBytes(), tx.ToBytes());

        // When
        var exception = Record
           .Exception(() => localSigner.ValidateSignature(testChannelId,
                                                          Bolt3AppendixCVectors.NodeBSignature0.ToCompact(), signedTx));

        // Then
        Assert.Null(exception);
    }

    [Fact]
    public void Given_InvalidChannelId_When_ValidatingSignature_Then_ThrowsException()
    {
        // Given
        var fundingOutputBuilderMock = new Mock<IFundingOutputBuilder>();
        var keyDerivationServiceMock = new Mock<IKeyDerivationService>();
        var loggerMock = new Mock<ILogger<LocalLightningSigner>>();
        var nodeOptions = new NodeOptions();
        var secureKeyManagerMock = new Mock<ISecureKeyManager>();

        var localSigner = new LocalLightningSigner(fundingOutputBuilderMock.Object, keyDerivationServiceMock.Object,
                                                   loggerMock.Object, nodeOptions, secureKeyManagerMock.Object);

        var unregisteredChannelId = ChannelId.Zero;
        var tx = Bolt3AppendixCVectors.ExpectedCommitTx0;
        var signedTx = new SignedTransaction(tx.GetHash().ToBytes(), tx.ToBytes());

        // When & Then
        Assert.Throws<SignerException>(() => localSigner.ValidateSignature(
                                           unregisteredChannelId, Bolt3AppendixCVectors.NodeBSignature0.ToCompact(),
                                           signedTx));
    }
}