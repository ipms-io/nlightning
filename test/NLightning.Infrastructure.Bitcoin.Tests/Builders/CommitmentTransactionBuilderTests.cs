using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Domain.Bitcoin.Transactions.Models;
using NLightning.Domain.Bitcoin.Transactions.Outputs;
using NLightning.Domain.Protocol.Models;
using NLightning.Tests.Utils.Mocks;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Infrastructure.Bitcoin.Tests.Builders;

using Domain.Money;
using Domain.Node.Options;
using Infrastructure.Bitcoin.Builders;

public class CommitmentTransactionBuilderTests
{
    [Fact]
    public void Given_ValidInput_When_Build_Then_ReturnsCorrectValues()
    {
        // Given
        var expectedTx = Bolt3AppendixCVectors.ExpectedCommitTx0;
        expectedTx.Inputs[0].WitScript = null;

        var sha256Mock = new Mock<FakeSha256>();
        sha256Mock.Setup(x => x.GetHashAndReset())
                  .Returns(Convert.FromHexString("C8BFEA84214B45899482A4BAD1D85C42130743ED78BA3711F5532BB038521914"));

        var nodeOptions = new NodeOptions();
        var builder = new CommitmentTransactionBuilder(new OptionsWrapper<NodeOptions>(nodeOptions));

        var commitmentNumber = new CommitmentNumber(Bolt3AppendixCVectors.NodeAPaymentBasepoint.ToBytes(),
                                                    Bolt3AppendixCVectors.NodeBPaymentBasepoint.ToBytes(),
                                                    sha256Mock.Object,
                                                    Bolt3AppendixCVectors.CommitmentNumber);
        var fundingOutputInfo = new FundingOutputInfo(Bolt3AppendixBVectors.FundingSatoshis,
                                                      Bolt3AppendixCVectors.NodeAFundingPubkey.ToBytes(),
                                                      Bolt3AppendixCVectors.NodeBFundingPubkey.ToBytes())
        {
            TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
            Index = 0,
        };

        var localOutput = new ToLocalOutputInfo(Bolt3AppendixCVectors.ExpectedCommitTx0ToLocalAmount,
                                                Bolt3AppendixCVectors.NodeADelayedPubkey.ToBytes(),
                                                Bolt3AppendixCVectors.NodeARevocationPubkey.ToBytes(),
                                                Bolt3AppendixCVectors.LocalDelay);
        var remoteOutput = new ToRemoteOutputInfo(Bolt3AppendixCVectors.ExpectedCommitTx0ToRemoteAmount,
                                                  Bolt3AppendixCVectors.NodeBPaymentBasepoint.ToBytes());
        var commitmentTransactionModel =
            new CommitmentTransactionModel(commitmentNumber, LightningMoney.Satoshis(15000), fundingOutputInfo, null,
                                           null, localOutput, remoteOutput);

        // When
        var unsignedTx = builder.Build(commitmentTransactionModel);

        // Then
        Assert.NotNull(unsignedTx);
        Assert.Equal(expectedTx.ToBytes(), unsignedTx.RawTxBytes);
    }
}