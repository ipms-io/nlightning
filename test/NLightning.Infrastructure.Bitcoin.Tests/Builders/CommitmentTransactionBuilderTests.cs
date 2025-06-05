using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Infrastructure.Crypto.Hashes;
using NLightning.Tests.Utils.Mocks;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Infrastructure.Bitcoin.Tests.Builders;

using Domain.Money;
using Domain.Node.Options;
using Domain.Protocol.ValueObjects;
using Domain.Transactions.Models;
using Domain.Transactions.Outputs;
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

    [Fact]
    public void Given_ValidInput_When_Build_Then_gwgReturnsCorrectValues()
    {
        // Given
        var expectedTx = Bolt3AppendixCVectors.ExpectedCommitTx0;
        expectedTx.Inputs[0].WitScript = null;

        var nodeOptions = new NodeOptions();
        var builder = new CommitmentTransactionBuilder(new OptionsWrapper<NodeOptions>(nodeOptions));

        var lndFundingPubKeyBytes =
            Convert.FromHexString("0295abdd93a819888780471f27d59600186c0e1f40b057e46298d4ee8d9cbbd555");
        var lndPaymentBasePointBytes =
            Convert.FromHexString("0268675ced5787297bbe20ef0c596cdfa7ccd3b02ba811f0efcd3ab430534496ed");
        var ourFundingPubKeyNytes =
            Convert.FromHexString("029db7f8d3483432c34b117cfd8c62aec07f1fde122061271f29f3977fa9a7706d");
        var ourDelayedPubKeyBytes =
            Convert.FromHexString("02f1f29e79ab51d10215faa831b2f68fceec5a6f0d043d200d19efd83a2ccd7b8f");
        var ourRevocationPubKeyBytes =
            Convert.FromHexString("023d27d1b6a8547a6390ff0702f80fc521b3c2e69559e4aba9ed2ff54712795764");

        var commitmentNumber = new CommitmentNumber(lndFundingPubKeyBytes, ourFundingPubKeyNytes, new Sha256());
        var fundingOutputInfo =
            new FundingOutputInfo(LightningMoney.Satoshis(20_000), ourFundingPubKeyNytes, lndFundingPubKeyBytes)
            {
                TransactionId =
                    Convert.FromHexString("7393d4b79cd75c797e106558adbb44dc2b3ef064755b41f19160af43ff4b334a"),
                Index = 0,
            };

        var remoteOutput = new ToRemoteOutputInfo(LightningMoney.Satoshis(10_950), lndPaymentBasePointBytes);
        var commitmentTransactionModel =
            new CommitmentTransactionModel(commitmentNumber, LightningMoney.Satoshis(15000), fundingOutputInfo, null,
                                           null, null, remoteOutput);

        // When
        var unsignedTx = builder.Build(commitmentTransactionModel);

        // Then
        Assert.NotNull(unsignedTx);
        Assert.Equal(expectedTx.ToBytes(), unsignedTx.RawTxBytes);
    }
}