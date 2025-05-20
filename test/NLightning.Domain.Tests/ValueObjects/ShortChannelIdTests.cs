namespace NLightning.Domain.Tests.ValueObjects;

using Domain.ValueObjects;

public class ShortChannelIdTests
{
    private const ulong EXPECTED_SHORT_CHANNEL_ID = 956714754222915585;
    private const uint EXPECTED_BLOCK_HEIGHT = 870127;
    private const uint EXPECTED_TX_INDEX = 1237;
    private const ushort EXPECTED_OUTPUT_INDEX = 1;
    private const string EXPECTED_STRING = "870127x1237x1";

    private readonly byte[] _expectedValue = [0x0D, 0x46, 0xEF, 0x00, 0x04, 0xD5, 0x00, 0x01];

    #region Constructor Tests
    [Fact]
    public void Given_ValidParameters_When_ConstructorCalled_Then_PropertiesAreSetCorrectly()
    {
        // Given
        // When
        var shortChannelId = new ShortChannelId(EXPECTED_BLOCK_HEIGHT, EXPECTED_TX_INDEX, EXPECTED_OUTPUT_INDEX);

        // Then
        Assert.Equal(EXPECTED_BLOCK_HEIGHT, shortChannelId.BlockHeight);
        Assert.Equal(EXPECTED_TX_INDEX, shortChannelId.TransactionIndex);
        Assert.Equal(EXPECTED_OUTPUT_INDEX, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }

    [Fact]
    public void Given_ValidByteArray_When_ConstructorCalled_Then_PropertiesAreExtractedCorrectly()
    {
        // Given
        // When
        var shortChannelId = new ShortChannelId(_expectedValue);

        // Then
        Assert.Equal(EXPECTED_BLOCK_HEIGHT, shortChannelId.BlockHeight);
        Assert.Equal(EXPECTED_TX_INDEX, shortChannelId.TransactionIndex);
        Assert.Equal(EXPECTED_OUTPUT_INDEX, shortChannelId.OutputIndex);
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
        var shortChannelId = new ShortChannelId(EXPECTED_SHORT_CHANNEL_ID);

        // Then
        Assert.Equal(EXPECTED_BLOCK_HEIGHT, shortChannelId.BlockHeight);
        Assert.Equal(EXPECTED_TX_INDEX, shortChannelId.TransactionIndex);
        Assert.Equal(EXPECTED_OUTPUT_INDEX, shortChannelId.OutputIndex);
        Assert.Equal(_expectedValue, shortChannelId);
    }
    #endregion

    #region Parse Tests
    [Fact]
    public void Given_ValidString_When_ParseCalled_Then_PropertiesMatch()
    {
        // Given
        // When
        var shortChannelId = ShortChannelId.Parse(EXPECTED_STRING);

        // Then
        Assert.Equal(EXPECTED_BLOCK_HEIGHT, shortChannelId.BlockHeight);
        Assert.Equal(EXPECTED_TX_INDEX, shortChannelId.TransactionIndex);
        Assert.Equal(EXPECTED_OUTPUT_INDEX, shortChannelId.OutputIndex);
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
        var shortChannelId = new ShortChannelId(EXPECTED_BLOCK_HEIGHT, EXPECTED_TX_INDEX, EXPECTED_OUTPUT_INDEX);

        // When
        var result = shortChannelId.ToString();

        // Then
        Assert.Equal(EXPECTED_STRING, result);
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