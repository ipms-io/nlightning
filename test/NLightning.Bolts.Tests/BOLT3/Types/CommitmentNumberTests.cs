using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Types;

using Bolts.BOLT3.Types;

public class CommitmentNumberTests
{
    private const ulong INITIAL_COMMITMENT_NUMBER = 42;
    private const ulong EXPECTED_OBSCURING_FACTOR = 0x2bb038521914UL;
    private const ulong EXPECTED_OBSCURED_VALUE = INITIAL_COMMITMENT_NUMBER ^ EXPECTED_OBSCURING_FACTOR;

    private readonly PubKey _localPaymentBasepoint = new PubKey("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _remotePaymentBasepoint = new PubKey("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");

    [Fact]
    public void Given_ValidParameters_When_ConstructingCommitmentNumber_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, INITIAL_COMMITMENT_NUMBER);

        // Then
        Assert.Equal(INITIAL_COMMITMENT_NUMBER, commitmentNumber.Value);
        Assert.NotEqual(0UL, commitmentNumber.ObscuringFactor);
    }

    [Fact]
    public void Given_CommitmentNumber_When_Increment_Then_ValueIsIncreased()
    {
        // Given
        const ulong EXPECTED_COMMITMENT_NUMBER = INITIAL_COMMITMENT_NUMBER + 1;
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, INITIAL_COMMITMENT_NUMBER);

        // When
        commitmentNumber.Increment();

        // Then
        Assert.Equal(EXPECTED_COMMITMENT_NUMBER, commitmentNumber.Value);
    }

    [Fact]
    public void Given_BOLT3TestVectors_When_CalculatingObscuringFactor_Then_MatchesExpectedValue()
    {
        // Given
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint);

        // When
        var obscuringFactor = commitmentNumber.ObscuringFactor;

        // Then - per BOLT3 test vectors, the obscuring factor should be 0x2bb038521914
        Assert.Equal(EXPECTED_OBSCURING_FACTOR, obscuringFactor);
    }

    [Fact]
    public void Given_CommitmentNumber_When_CalculatingObscuredValue_Then_ReturnsXORedValue()
    {
        // Given
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, INITIAL_COMMITMENT_NUMBER);

        // When
        var obscuredValue = commitmentNumber.ObscuredValue;

        // Then
        Assert.Equal(EXPECTED_OBSCURED_VALUE, obscuredValue);
    }

    [Fact]
    public void Given_CommitmentNumber_When_CalculateLockTime_Then_ReturnsCorrectValue()
    {
        // Given
        const uint EXPECTED_LOCKTIME = (uint)((0x20 << 24) | (EXPECTED_OBSCURED_VALUE & 0xFFFFFF));
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, INITIAL_COMMITMENT_NUMBER);

        // When
        var lockTime = commitmentNumber.CalculateLockTime();

        // Then - formula is (0x20 << 24) | (obscured & 0xFFFFFF)
        Assert.Equal(EXPECTED_LOCKTIME, lockTime.Value);
    }

    [Fact]
    public void Given_CommitmentNumber_When_CalculateSequence_Then_ReturnsCorrectValue()
    {
        // Given
        const uint EXPECTED_SEQUENCE = (uint)((0x80U << 24) | ((EXPECTED_OBSCURED_VALUE >> 24) & 0xFFFFFF));
        var commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, INITIAL_COMMITMENT_NUMBER);

        // When
        var sequence = commitmentNumber.CalculateSequence();

        // Then - formula is (0x80 << 24) | ((obscured >> 24) & 0xFFFFFF)
        Assert.Equal(EXPECTED_SEQUENCE, sequence.Value);
    }
}