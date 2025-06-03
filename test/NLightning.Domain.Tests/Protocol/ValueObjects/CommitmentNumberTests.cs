using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Infrastructure.Crypto.Hashes;

namespace NLightning.Domain.Tests.Protocol.ValueObjects;

public class CommitmentNumberTests
{
    private const ulong InitialCommitmentNumber = 42;
    private const ulong ExpectedObscuringFactor = 0x2bb038521914UL;
    private const ulong ExpectedObscuredValue = InitialCommitmentNumber ^ ExpectedObscuringFactor;

    private readonly CompactPubKey _localPaymentBasepoint =
        Convert.FromHexString("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");

    private readonly CompactPubKey _remotePaymentBasepoint =
        Convert.FromHexString("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");

    [Fact]
    public void Given_ValidParameters_When_ConstructingCommitmentNumber_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var commitmentNumber =
            new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, new Sha256(),
                                 InitialCommitmentNumber);

        // Then
        Assert.Equal(InitialCommitmentNumber, commitmentNumber.Value);
        Assert.NotEqual(0UL, commitmentNumber.ObscuringFactor);
    }

    // [Fact]
    // public void Given_CommitmentNumber_When_Increment_Then_ValueIsIncreased()
    // {
    //     // Given
    //     const ulong expectedCommitmentNumber = InitialCommitmentNumber + 1;
    //     var commitmentNumber =
    //         new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, InitialCommitmentNumber);
    //
    //     // When
    //     commitmentNumber.Increment();
    //
    //     // Then
    //     Assert.Equal(expectedCommitmentNumber, commitmentNumber.Value);
    // }
    //
    // [Fact]
    // public void Given_BOLT3TestVectors_When_CalculatingObscuringFactor_Then_MatchesExpectedValue()
    // {
    //     // Given
    //     var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint);
    //
    //     // When
    //     var obscuringFactor = commitmentNumber.ObscuringFactor;
    //
    //     // Then - per BOLT3 test vectors, the obscuring factor should be 0x2bb038521914
    //     Assert.Equal(ExpectedObscuringFactor, obscuringFactor);
    // }
    //
    // [Fact]
    // public void Given_CommitmentNumber_When_CalculatingObscuredValue_Then_ReturnsXORedValue()
    // {
    //     // Given
    //     var commitmentNumber =
    //         new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, InitialCommitmentNumber);
    //
    //     // When
    //     var obscuredValue = commitmentNumber.ObscuredValue;
    //
    //     // Then
    //     Assert.Equal(ExpectedObscuredValue, obscuredValue);
    // }
    //
    // [Fact]
    // public void Given_CommitmentNumber_When_CalculateLockTime_Then_ReturnsCorrectValue()
    // {
    //     // Given
    //     const uint expectedLocktime = (uint)((0x20 << 24) | (ExpectedObscuredValue & 0xFFFFFF));
    //     var commitmentNumber =
    //         new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, InitialCommitmentNumber);
    //
    //     // When
    //     var lockTime = commitmentNumber.CalculateLockTime();
    //
    //     // Then - formula is (0x20 << 24) | (obscured & 0xFFFFFF)
    //     Assert.Equal(expectedLocktime, lockTime.Value);
    // }
    //
    // [Fact]
    // public void Given_CommitmentNumber_When_CalculateSequence_Then_ReturnsCorrectValue()
    // {
    //     // Given
    //     const uint expectedSequence = (uint)((0x80U << 24) | ((ExpectedObscuredValue >> 24) & 0xFFFFFF));
    //     var commitmentNumber =
    //         new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, InitialCommitmentNumber);
    //
    //     // When
    //     var sequence = commitmentNumber.CalculateSequence();
    //
    //     // Then - formula is (0x80 << 24) | ((obscured >> 24) & 0xFFFFFF)
    //     Assert.Equal(expectedSequence, sequence.Value);
    // }
}