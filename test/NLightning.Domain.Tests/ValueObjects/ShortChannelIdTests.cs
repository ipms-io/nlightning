namespace NLightning.Domain.Tests.ValueObjects;

using Domain.Channels.ValueObjects;

public class ShortChannelIdTests
{
    private const ulong ExpectedShortChannelId = 956714754222915585;
    private const uint ExpectedBlockHeight = 870127;
    private const uint ExpectedTxIndex = 1237;
    private const ushort ExpectedOutputIndex = 1;
    private const string ExpectedString = "870127x1237x1";

    private readonly byte[] _expectedValue = [0x0D, 0x46, 0xEF, 0x00, 0x04, 0xD5, 0x00, 0x01];

    #region Constructor Tests

    [Fact]
    public void Given_ValidParameters_When_ConstructorCalled_Then_PropertiesAreSetCorrectly()
    {
        // Given
        // When
        var shortChannelId = new ShortChannelId(ExpectedBlockHeight, ExpectedTxIndex, ExpectedOutputIndex);

        // Then
        Assert.Equal(ExpectedBlockHeight, shortChannelId.BlockHeight);
        Assert.Equal(ExpectedTxIndex, shortChannelId.TransactionIndex);
        Assert.Equal(ExpectedOutputIndex, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }

    [Fact]
    public void Given_ValidByteArray_When_ConstructorCalled_Then_PropertiesAreExtractedCorrectly()
    {
        // Given
        // When
        var shortChannelId = new ShortChannelId(_expectedValue);

        // Then
        Assert.Equal(ExpectedBlockHeight, shortChannelId.BlockHeight);
        Assert.Equal(ExpectedTxIndex, shortChannelId.TransactionIndex);
        Assert.Equal(ExpectedOutputIndex, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }

    [Fact]
    public void Given_InvalidByteArrayLength_When_ConstructorCalled_Then_ArgumentExceptionIsThrown()
    {
        // Given
        var invalidByteArray = new byte[] { 0x01, 0x02 }; // only 2 bytes, should be 8

        // When / Then
        var exception = Assert.Throws<ArgumentException>(() => new ShortChannelId(invalidByteArray));
        Assert.Contains("ShortChannelId must be 8 bytes", exception.Message);
    }

    [Fact]
    public void Given_ValidUlong_When_ConstructorCalled_Then_PropertiesAreExtractedCorrectly()
    {
        // Given
        // When
        var shortChannelId = new ShortChannelId(ExpectedShortChannelId);

        // Then
        Assert.Equal(ExpectedBlockHeight, shortChannelId.BlockHeight);
        Assert.Equal(ExpectedTxIndex, shortChannelId.TransactionIndex);
        Assert.Equal(ExpectedOutputIndex, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }

    #endregion

    #region Parse Tests

    [Fact]
    public void Given_ValidString_When_ParseCalled_Then_PropertiesMatch()
    {
        // Given
        // When
        var shortChannelId = ShortChannelId.Parse(ExpectedString);

        // Then
        Assert.Equal(ExpectedBlockHeight, shortChannelId.BlockHeight);
        Assert.Equal(ExpectedTxIndex, shortChannelId.TransactionIndex);
        Assert.Equal(ExpectedOutputIndex, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }

    [Fact]
    public void Given_InvalidStringFormat_When_ParseCalled_Then_FormatExceptionIsThrown()
    {
        // Given
        var invalidString = "this-is-not-valid";

        // When / Then
        Assert.Throws<FormatException>(() => ShortChannelId.Parse(invalidString));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void Given_ValidShortChannelId_When_ToStringCalled_Then_FormattedCorrectly()
    {
        // Given
        var shortChannelId = new ShortChannelId(ExpectedBlockHeight, ExpectedTxIndex, ExpectedOutputIndex);

        // When
        var result = shortChannelId.ToString();

        // Then
        Assert.Equal(ExpectedString, result);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Given_TwoIdenticalShortChannelIds_When_ComparingEquality_Then_TheyAreEqual()
    {
        // Given
        var scid1 = new ShortChannelId(123, 45, 6);
        var scid2 = new ShortChannelId(123, 45, 6);

        // When
        var areEqual = scid1 == scid2;

        // Then
        Assert.True(areEqual);
        Assert.True(scid1.Equals(scid2));
        Assert.Equal(scid1.GetHashCode(), scid2.GetHashCode());
    }

    [Fact]
    public void Given_TwoDifferentShortChannelIds_When_ComparingEquality_Then_TheyAreNotEqual()
    {
        // Given
        var scid1 = new ShortChannelId(123, 45, 6);
        var scid2 = new ShortChannelId(321, 54, 6);

        // When
        var areEqual = scid1 == scid2;

        // Then
        Assert.False(areEqual);
        Assert.False(scid1.Equals(scid2));
    }

    #endregion
}