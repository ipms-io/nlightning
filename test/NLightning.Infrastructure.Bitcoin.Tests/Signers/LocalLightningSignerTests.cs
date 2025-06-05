using Microsoft.Extensions.Logging;
using NBitcoin;
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
                                .Returns(new FundingOutput(Bolt3AppendixBVectors.FundingSatoshis,
                                                           new PubKey(
                                                               Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes()),
                                                           new PubKey(
                                                               Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes()))
                                {
                                    TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
                                    Index = 0,
                                });
        var keyDerivationServiceMock = new Mock<IKeyDerivationService>();
        var loggerMock = new Mock<ILogger<LocalLightningSigner>>();
        var nodeOptions = new NodeOptions();
        var secureKeyManagerMock = new Mock<ISecureKeyManager>();

        var localSigner = new LocalLightningSigner(fundingOutputBuilderMock.Object, keyDerivationServiceMock.Object,
                                                   loggerMock.Object, nodeOptions, secureKeyManagerMock.Object);

        var tx = Bolt3AppendixCVectors.ExpectedCommitTx0;
        tx.Inputs[0].WitScript = null;
        var signedTx = new SignedTransaction(tx.GetHash().ToBytes(), tx.ToBytes())
        {
            OutputInfo = new FundingOutputInfo(Bolt3AppendixBVectors.FundingSatoshis,
                                                        Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes(),
                                                        Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes())
            {
                TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
                Index = 0,
            }
        };

        // When
        var isValid = localSigner.ValidateSignature(Bolt3AppendixCVectors.NodeBSignature0.ToCompact(),
                                                    Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes(), signedTx);

        // Then
        Assert.True(isValid, "The signature should be valid for the given transaction and public key.");
    }
}