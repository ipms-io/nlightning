using System.Reflection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.Policy;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Domain.Transactions.Models;
using NLightning.Infrastructure.Bitcoin.Builders;
using NLightning.Infrastructure.Bitcoin.Services;
using NLightning.Infrastructure.Crypto.Hashes;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Integration.Tests.BOLT3;

using Domain.Enums;
using Domain.Money;
using Domain.Node.Options;
using Infrastructure.Bitcoin.Factories;
using Infrastructure.Bitcoin.Outputs;
using Infrastructure.Bitcoin.Transactions;
using Infrastructure.Protocol.Models;
using Infrastructure.Protocol.Services;

public class Bolt3IntegrationTests
{
    private static readonly Sha256 s_sha256 = new();

    private OfferedHtlcOutput? _offeredHtlc2;
    private OfferedHtlcOutput? _offeredHtlc3;
    private OfferedHtlcOutput? _offeredHtlc5;
    private OfferedHtlcOutput? _offeredHtlc6;
    private ReceivedHtlcOutput? _receivedHtlc0;
    private ReceivedHtlcOutput? _receivedHtlc1;
    private ReceivedHtlcOutput? _receivedHtlc4;

    private readonly CommitmentNumber _commitmentNumber = new(Bolt3AppendixCVectors.NodeAPaymentBasepoint.ToBytes(),
                                                              Bolt3AppendixCVectors.NodeBPaymentBasepoint.ToBytes(),
                                                              s_sha256,
                                                              Bolt3AppendixCVectors.CommitmentNumber);

    private readonly FundingOutput _fundingOutput = new(Bolt3AppendixBVectors.FundingSatoshis,
                                                        Bolt3AppendixCVectors.NodeAFundingPubkey,
                                                        Bolt3AppendixCVectors.NodeBFundingPubkey)
    {
        TransactionId = Bolt3AppendixBVectors.ExpectedTxId.ToBytes(),
        Index = 0
    };

    private readonly StandardTransactionPolicy _dontCheckFeePolicy = new()
    {
        CheckFee = false
    };

    private readonly LightningMoney _anchorAmount = LightningMoney.Satoshis(330);

    #region Appendix B Vectors

    // [Fact]
    // public void Given_Bolt3Specifications_When_CreatingFundingTransaction_Then_ShouldBeEqualToTestVector()
    // {
    //     // Given
    //     var nodeOptions = Options.Create(new NodeOptions
    //     {
    //         HasAnchorOutputs = false
    //     });
    //
    //     var feeServiceMock = new Mock<IFeeService>();
    //     feeServiceMock
    //        .Setup(x => x.GetCachedFeeRatePerKw())
    //        .Returns(new LightningMoney(15000, LightningMoneyUnit.Satoshi));
    //     var fundingTransactionFactory =
    //         new FundingTransactionFactory(feeServiceMock.Object, nodeOptions, _lightningSigner);
    //
    //     var fundingInputCoin = new Coin(AppendixBVectors.InputTx, AppendixBVectors.InputIndex);
    //
    //     // When
    //     var fundingTransaction = fundingTransactionFactory
    //        .CreateFundingTransaction(AppendixBVectors.LocalPubKey, AppendixBVectors.RemotePubKey,
    //                                  AppendixBVectors.FundingSatoshis, AppendixBVectors.ChangeScript.PaymentScript,
    //                                  AppendixBVectors.ChangeScript, [fundingInputCoin],
    //                                  new BitcoinSecret(AppendixBVectors.InputSigningPrivKey, Network.Main));
    //     var finalFundingTx = fundingTransaction.GetSignedTransaction();
    //
    //     // Then
    //     Assert.Equal(AppendixBVectors.ExpectedTx.ToBytes(), finalFundingTx.ToBytes());
    //     Assert.True(fundingTransaction.IsValid);
    // }
    //

    #endregion

    #region Appendix C Vectors

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            HasAnchorOutputs = false
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(15000, LightningMoneyUnit.Satoshi));
        var commitmentTransactionBuilder = new CommitmentTransactionBuilder(nodeOptions);

        // When
        var commitmentTransaction = commitmentTransactionBuilder.Build(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint, Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey, Bolt3AppendixCVectors.NodeARevocationPubkey,
            Bolt3AppendixCVectors.Tx0ToLocalMsat, Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransaction
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature0, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransaction.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx0.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransaction.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5HTLCsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(LightningMoney.Zero);
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc0!, _receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint, Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay, _commitmentNumber,
            true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        // Allow zero fee via reflection
        var builder = typeof(BaseTransaction)
                     .GetField("_builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                     .GetValue(commitmentTransacion);
        var propertyInfo = typeof(TransactionBuilder)
           .GetProperty("StandardTransactionPolicy", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo?.SetValue(builder, _dontCheckFeePolicy);

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature1, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx1.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith7OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(647, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc0!, _receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature2, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx2.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith6OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(648, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature3, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx3.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith6OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(2069, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion.AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature4,
                                                          _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx4.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(2070, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature5, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx5.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(2194, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature6, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx6.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith4OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(2195, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion.AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature7,
                                                          _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx7.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith4OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(3702, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature8, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx8.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith3OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(3703, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, [], receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature9, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx9.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith3OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(4914, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, [], receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature10, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx10.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(4915, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature11, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx11.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(9651180, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        // Allow huge fee via reflection
        var builder = typeof(BaseTransaction)
                     .GetField("_builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                     .GetValue(commitmentTransacion);
        var propertyInfo = typeof(TransactionBuilder)
           .GetProperty("StandardTransactionPolicy", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo?.SetValue(builder, _dontCheckFeePolicy);

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature12, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx12.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith1OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw())
                      .Returns(new LightningMoney(9651181, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        // Allow huge fee via reflection
        var builder = typeof(BaseTransaction)
                     .GetField("_builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                     .GetValue(commitmentTransacion);
        var propertyInfo = typeof(TransactionBuilder)
           .GetProperty("StandardTransactionPolicy", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo?.SetValue(builder, _dontCheckFeePolicy);

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature13, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx13.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithFeeGreaterThanFunderAmount_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(9651936, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx1ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        // Allow huge fee via reflection
        var builder = typeof(BaseTransaction)
                     .GetField("_builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                     .GetValue(commitmentTransacion);
        var propertyInfo = typeof(TransactionBuilder)
           .GetProperty("StandardTransactionPolicy", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo?.SetValue(builder, _dontCheckFeePolicy);

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature14, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx14.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2SimilarOfferedHtlc_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = LightningMoney.Zero
        });
        GenerateHtlcs(LightningMoney.Zero);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(253, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc5!, _offeredHtlc6!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc1!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixCVectors.Tx15ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixCVectors.NodeBSignature15, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixCVectors.ExpectedCommitTx15.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    #endregion

    #region Appendix D Vectors

    #region Generation Tests

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeed0FinalNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(Bolt3AppendixDVectors.Seed0FinalNode,
                                                                      Bolt3AppendixDVectors.I0FinalNode);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.ExpectedOutput0FinalNode, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFFinalNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(Bolt3AppendixDVectors.SeedFfFinalNode,
                                                                      Bolt3AppendixDVectors.IFfFinalNode);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.ExpectedOutputFfFinalNode, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFAlternateBits1_Then_ShouldBeEqualToTestVector()
    {
        // Given
        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(Bolt3AppendixDVectors.SeedFfAlternateBits1,
                                                                      Bolt3AppendixDVectors.IFfAlternateBits1);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.ExpectedOutputFfAlternateBits1, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFAlternateBits2_Then_ShouldBeEqualToTestVector()
    {
        // Given
        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(Bolt3AppendixDVectors.SeedFfAlternateBits2,
                                                                      Bolt3AppendixDVectors.IFfAlternateBits2);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.ExpectedOutputFfAlternateBits2, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeed01LastNonTrivialNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(Bolt3AppendixDVectors.Seed01LastNonTrivialNode,
                                                                      Bolt3AppendixDVectors.I01LastNonTrivialNode);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.ExpectedOutput01LastNonTrivialNode, result);
    }

    #endregion

    #region Storage Tests

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecretsInCorrectSequence_Then_ShouldSucceed()
    {
        // Given
        using var storage = new SecretStorageService();

        // When
        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret4, Bolt3AppendixDVectors.StorageIndexMax - 4);
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret5, Bolt3AppendixDVectors.StorageIndexMax - 5);
        var result7 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret6, Bolt3AppendixDVectors.StorageIndexMax - 6);
        var result8 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret7, Bolt3AppendixDVectors.StorageIndexMax - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.True(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets1InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret8, Bolt3AppendixDVectors.StorageIndexMax);

        // When
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);

        // Then
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets2InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret8, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret9, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);

        // When
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.False(result4);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets3InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret10, Bolt3AppendixDVectors.StorageIndexMax - 2);

        // When
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.False(result4);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets4InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret8, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret9, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret10, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret11, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret4, Bolt3AppendixDVectors.StorageIndexMax - 4);
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret5, Bolt3AppendixDVectors.StorageIndexMax - 5);
        var result7 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret6, Bolt3AppendixDVectors.StorageIndexMax - 6);

        // When
        var result8 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret7, Bolt3AppendixDVectors.StorageIndexMax - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets5InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret12, Bolt3AppendixDVectors.StorageIndexMax - 4);

        // When
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret5, Bolt3AppendixDVectors.StorageIndexMax - 5);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.False(result6);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets6InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret12, Bolt3AppendixDVectors.StorageIndexMax - 4);
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret13, Bolt3AppendixDVectors.StorageIndexMax - 5);
        var result7 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret6, Bolt3AppendixDVectors.StorageIndexMax - 6);

        // When
        var result8 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret7, Bolt3AppendixDVectors.StorageIndexMax - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets7InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret4, Bolt3AppendixDVectors.StorageIndexMax - 4);
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret5, Bolt3AppendixDVectors.StorageIndexMax - 5);
        var result7 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret14, Bolt3AppendixDVectors.StorageIndexMax - 6);

        // When
        var result8 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret7, Bolt3AppendixDVectors.StorageIndexMax - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets8InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        var result2 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        var result3 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);
        var result4 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret3, Bolt3AppendixDVectors.StorageIndexMax - 3);
        var result5 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret4, Bolt3AppendixDVectors.StorageIndexMax - 4);
        var result6 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret5, Bolt3AppendixDVectors.StorageIndexMax - 5);
        var result7 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret6, Bolt3AppendixDVectors.StorageIndexMax - 6);

        // When
        var result8 = storage
           .InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret15, Bolt3AppendixDVectors.StorageIndexMax - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingOldSecret_Then_ShouldDeriveCorrectly()
    {
        // Given
        using var storage = new SecretStorageService();
        Span<byte> derivedSecret = stackalloc byte[SecretStorageService.SecretSize];

        // Insert a valid secret with known index
        storage.InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret0, Bolt3AppendixDVectors.StorageIndexMax);
        storage.InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax - 1);
        storage.InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret2, Bolt3AppendixDVectors.StorageIndexMax - 2);

        // When
        storage.DeriveOldSecret(Bolt3AppendixDVectors.StorageIndexMax, derivedSecret);

        // Then
        Assert.Equal(Bolt3AppendixDVectors.StorageExpectedSecret0, derivedSecret);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingUnknownSecret_Then_ShouldThrowException()
    {
        // Given
        using var storage = new SecretStorageService();
        var derivedSecret = new byte[SecretStorageService.SecretSize];

        // Insert a valid secret with known index
        storage.InsertSecret(Bolt3AppendixDVectors.StorageExpectedSecret1, Bolt3AppendixDVectors.StorageIndexMax);

        // When/Then
        // Cannot derive a secret with higher index
        Assert.Throws<InvalidOperationException>(() =>
        {
            storage.DeriveOldSecret(Bolt3AppendixDVectors.StorageIndexMax - 1, derivedSecret);
        });
    }

    [Fact]
    public void Given_Bolt3Specifications_When_StoringAndDeriving48Secrets_Then_ShouldWorkCorrectly()
    {
        // Given
        using var storage = new SecretStorageService();
        Span<byte> derivedSecret = stackalloc byte[SecretStorageService.SecretSize];

        // Insert secrets with different number of trailing zeros
        for (var i = 0; i < 48; i++)
        {
            var index = (Bolt3AppendixDVectors.StorageIndexMax - 1) & ~((1UL << i) - 1);
            var secret = KeyDerivationService.GeneratePerCommitmentSecret(
                Bolt3AppendixDVectors.StorageCorrectSeed, index);

            storage.InsertSecret(secret, index);
        }

        // When/Then
        // We should be able to derive any secret in the range
        for (var i = 1U; i < 20; i++)
        {
            var index = Bolt3AppendixDVectors.StorageIndexMax - i;
            var expectedSecret = KeyDerivationService.GeneratePerCommitmentSecret(
                Bolt3AppendixDVectors.StorageCorrectSeed, index);

            storage.DeriveOldSecret(index, derivedSecret);

            Assert.Equal(expectedSecret, derivedSecret);
        }
    }

    #endregion

    #endregion

    #region Appendix E Vectors

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingPubKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var basepoint = new PubKey("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2");
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedLocalPubkey = new PubKey("0235f2dbfaa89b57ec7b055afe29849ef7ddfeb1cefdb9ebdc43f5494984db29e5");

        // When
        var result = keyDerivationService.DerivePublicKey(basepoint, perCommitmentPoint);

        // Then
        Assert.Equal(expectedLocalPubkey, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingPrivateKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var baseSecretBytes = Convert.FromHexString("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var basepointSecret = new Key(baseSecretBytes);
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedPrivkeyBytes = Convert
           .FromHexString("cbced912d3b21bf196a766651e436aff192362621ce317704ea2f75d87e7be0f");

        // When
        var result = keyDerivationService.DerivePrivateKey(basepointSecret, perCommitmentPoint);

        // Then
        Assert.Equal(expectedPrivkeyBytes, result.ToBytes());
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingRevocationPubKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var revocationBasepoint = new PubKey("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2");
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedRevocationPubkey = new PubKey("02916e326636d19c33f13e8c0c3a03dd157f332f3e99c317c141dd865eb01f8ff0");

        // When
        var result = keyDerivationService.DeriveRevocationPubKey(revocationBasepoint, perCommitmentPoint);

        // Then
        Assert.Equal(expectedRevocationPubkey, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingRevocationPrivKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var baseSecretBytes = Convert.FromHexString("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var revocationBasepointSecret = new Key(baseSecretBytes);
        var perCommitmentSecretBytes = Convert
           .FromHexString("1f1e1d1c1b1a191817161514131211100f0e0d0c0b0a09080706050403020100");
        var perCommitmentSecret = new Key(perCommitmentSecretBytes);
        var expectedRevocationPrivkeyBytes = Convert
           .FromHexString("d09ffff62ddb2297ab000cc85bcb4283fdeb6aa052affbc9dddcf33b61078110");

        // When
        var result = keyDerivationService.DeriveRevocationPrivKey(revocationBasepointSecret, perCommitmentSecret);

        // Then
        Assert.Equal(expectedRevocationPrivkeyBytes, result.ToBytes());
    }

    #endregion

    #region Vector F Tests

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchors_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(15000, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransaction = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx0ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransaction
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature0, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransaction.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx0.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransaction.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithSingleAnchor_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(15000, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransaction = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx1ToLocalMsat,
            0UL, Bolt3AppendixCVectors.LocalDelay, _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransaction
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature1, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransaction.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx1.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransaction.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd7OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount
        });
        GenerateHtlcs(_anchorAmount);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(644, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc0!, _receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature2, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx2.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd6OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
            DustLimitAmount = LightningMoney.Satoshis(1_001)
        });
        GenerateHtlcs(_anchorAmount);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(645, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc2!, _offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc1!, _receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature3, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx3.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd4OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
            DustLimitAmount = LightningMoney.Satoshis(2_001)
        });
        GenerateHtlcs(_anchorAmount);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(2185, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc3!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature4, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx4.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd3OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
            DustLimitAmount = LightningMoney.Satoshis(3_001)
        });
        GenerateHtlcs(_anchorAmount);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(3687, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc4!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, [], receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature5, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx5.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd2OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
            DustLimitAmount = LightningMoney.Satoshis(4_001)
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(4894, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature6, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx6.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd1OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
            DustLimitAmount = LightningMoney.Satoshis(4_001)
        });

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(6_216_010, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx2ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        // Allow huge fee via reflection
        var builder = typeof(BaseTransaction)
                     .GetField("_builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                     .GetValue(commitmentTransacion);
        var propertyInfo = typeof(TransactionBuilder)
           .GetProperty("StandardTransactionPolicy", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo?.SetValue(builder, _dontCheckFeePolicy);

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature7, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx7.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    [Fact]
    public void
        Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithAnchorsAnd2SimilarOfferedHtlc_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var nodeOptions = Options.Create(new NodeOptions
        {
            AnchorAmount = _anchorAmount,
        });
        GenerateHtlcs(_anchorAmount);

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock
           .Setup(x => x.GetCachedFeeRatePerKw())
           .Returns(new LightningMoney(253, LightningMoneyUnit.Satoshi));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object, nodeOptions);

        List<OfferedHtlcOutput> offeredHtlcs = [_offeredHtlc5!, _offeredHtlc6!];
        List<ReceivedHtlcOutput> receivedHtlcs = [_receivedHtlc1!];

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(
            _fundingOutput, Bolt3AppendixCVectors.NodeAPaymentBasepoint,
            Bolt3AppendixCVectors.NodeBPaymentBasepoint,
            Bolt3AppendixCVectors.NodeADelayedPubkey,
            Bolt3AppendixCVectors.NodeARevocationPubkey, Bolt3AppendixFVectors.Tx8ToLocalMsat,
            Bolt3AppendixCVectors.ToRemoteMsat, Bolt3AppendixCVectors.LocalDelay,
            _commitmentNumber, true, offeredHtlcs, receivedHtlcs,
            new BitcoinSecret(Bolt3AppendixCVectors.NodeAFundingPrivkey, Network.Main));

        commitmentTransacion
           .AppendRemoteSignatureAndSign(Bolt3AppendixFVectors.NodeBSignature8, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(Bolt3AppendixFVectors.ExpectedCommitTx8.ToBytes(), finalCommitmentTx.ToBytes());
        Assert.True(commitmentTransacion.IsValid);
    }

    #endregion

    private void GenerateHtlcs(LightningMoney anchorAmount)
    {
        _offeredHtlc2 = new OfferedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                              Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                              Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                              Bolt3AppendixCVectors.Htlc2PaymentHash, 2_000_000UL, 502);
        _offeredHtlc3 = new OfferedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                              Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                              Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                              Bolt3AppendixCVectors.Htlc3PaymentHash, 3_000_000UL, 503);
        _offeredHtlc5 = new OfferedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                              Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                              Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                              Bolt3AppendixCVectors.Htlc5PaymentHash, 5_000_000UL, 506);
        _offeredHtlc6 = new OfferedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                              Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                              Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                              Bolt3AppendixCVectors.Htlc6PaymentHash, 5_000_000UL, 505);

        _receivedHtlc0 = new ReceivedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                                Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                                Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                                Bolt3AppendixCVectors.Htlc0PaymentHash, 1_000_000UL, 500);
        _receivedHtlc1 = new ReceivedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                                Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                                Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                                Bolt3AppendixCVectors.Htlc1PaymentHash, 2_000_000UL, 501);
        _receivedHtlc4 = new ReceivedHtlcOutput(anchorAmount, Bolt3AppendixCVectors.NodeARevocationPubkey,
                                                Bolt3AppendixCVectors.NodeBHtlcPubkey,
                                                Bolt3AppendixCVectors.NodeAHtlcPubkey,
                                                Bolt3AppendixCVectors.Htlc4PaymentHash, 4_000_000UL, 504);
    }
}