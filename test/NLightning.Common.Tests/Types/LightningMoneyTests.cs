using NBitcoin;

namespace NLightning.Common.Tests.Types;

using Common.Types;
using Enums;

public class LightningMoneyTests
{
    #region Constructors
    [Fact]
    public void Given_ValidMilliSatoshi_When_Constructed_Then_PropertiesAreSetCorrectly()
    {
        // Given
        const ulong MILLI_SATOSHI = 1000;

        // When
        var lightningMoney = new LightningMoney(MILLI_SATOSHI);

        // Then
        Assert.Equal(MILLI_SATOSHI, lightningMoney.MilliSatoshi);
        Assert.Equal(1, lightningMoney.Satoshi);
    }

    [Fact]
    public void Given_ValidDecimalAmountAndUnit_When_Constructed_Then_PropertiesAreSetCorrectly()
    {
        // Given
        const decimal AMOUNT = 1.5m;
        const LightningMoneyUnit UNIT = LightningMoneyUnit.BTC;

        // When
        var lightningMoney = new LightningMoney(AMOUNT, UNIT);

        // Then
        Assert.Equal(150000000000UL, lightningMoney.MilliSatoshi);
    }

    [Fact]
    public void Given_ValidLongAmountAndUnit_When_Constructed_Then_PropertiesAreSetCorrectly()
    {
        // Given
        const long AMOUNT = 1;
        const LightningMoneyUnit UNIT = LightningMoneyUnit.BTC;

        // When
        var lightningMoney = new LightningMoney(AMOUNT, UNIT);

        // Then
        Assert.Equal(100000000000UL, lightningMoney.MilliSatoshi);
    }

    [Fact]
    public void Given_ValidULongAmountAndUnit_When_Constructed_Then_PropertiesAreSetCorrectly()
    {
        // Given
        const ulong AMOUNT = 1;
        const LightningMoneyUnit UNIT = LightningMoneyUnit.BTC;

        // When
        var lightningMoney = new LightningMoney(AMOUNT, UNIT);

        // Then
        Assert.Equal(100000000000UL, lightningMoney.MilliSatoshi);
    }

    [Fact]
    public void Given_NegativeSatoshi_When_Constructed_Then_ThrowsArgumentOutOfRangeException()
    {
        // Given
        const decimal AMOUNT = -1m;
        const LightningMoneyUnit UNIT = LightningMoneyUnit.BTC;

        // When & Then
        Assert.Throws<OverflowException>(() => new LightningMoney(AMOUNT, UNIT));
    }

    [Fact]
    public void Given_NegativeSatoshi_When_Set_Then_ThrowsArgumentOutOfRangeException()
    {
        // Given
        var lightningMoney = new LightningMoney(1000);

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => lightningMoney.Satoshi = -1);
    }
    #endregion

    #region Public Properties

    [Fact]
    public void Given_ValidLightningMoney_When_SatoshiSet_Then_PropertiesAreSetCorrectly()
    {
        // Given
        var lightningMoney = new LightningMoney(1000);
        const long SATOSHI = 1;

        // When
        lightningMoney.Satoshi = SATOSHI;

        // Then
        Assert.Equal(SATOSHI, lightningMoney.Satoshi);
    }
    #endregion

    #region Parse
    [Fact]
    public void Given_ValidString_When_Parsed_Then_ReturnsLightningMoney()
    {
        // Given
        const string BITCOIN_AMOUNT = "0.001";

        // When
        var lightningMoney = LightningMoney.Parse(BITCOIN_AMOUNT);

        // Then
        Assert.NotNull(lightningMoney);
        Assert.Equal(100_000L, lightningMoney.Satoshi);
    }

    [Fact]
    public void Given_InvalidString_When_Parsed_Then_ThrowsFormatException()
    {
        // Given
        const string INVALID_BITCOIN_AMOUNT = "invalid";

        // When & Then
        Assert.Throws<FormatException>(() => LightningMoney.Parse(INVALID_BITCOIN_AMOUNT));
    }

    [Fact]
    public void Given_ValidString_When_TryParse_Then_ReturnsLightningMoney()
    {
        // Given
        const string BITCOIN_AMOUNT = "0.001";

        // When
        var isParsed = LightningMoney.TryParse(BITCOIN_AMOUNT, out var result);

        // Then
        Assert.True(isParsed);
        Assert.NotNull(result);
        Assert.Equal(100_000L, result.Satoshi);
    }

    [Fact]
    public void Given_InvalidString_When_TryParse_Then_ThrowsFormatException()
    {
        // Given
        const string INVALID_BITCOIN_AMOUNT = "invalid";

        // When & Then
        var isParsed = LightningMoney.TryParse(INVALID_BITCOIN_AMOUNT, out var result);

        Assert.False(isParsed);
        Assert.Null(result);
    }
    #endregion

    #region Split, Add, Subtract
    [Fact]
    public void Given_LightningMoney_When_Split_Then_ReturnsCorrectParts()
    {
        // Given
        var lightningMoney = new LightningMoney(1000);

        // When
        var parts = lightningMoney.Split(3);

        // Then
        var partsArray = parts.ToArray();
        Assert.Equal(3, partsArray.Length);
        Assert.Equal(334UL, partsArray[0].MilliSatoshi);
        Assert.Equal(333UL, partsArray[1].MilliSatoshi);
        Assert.Equal(333UL, partsArray[2].MilliSatoshi);
    }

    [Fact]
    public void Given_TwoLightningMoneyInstances_When_Added_Then_ReturnsCorrectSum()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(2000);

        // When
        var result = money1 + money2;

        // Then
        Assert.Equal(3000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_TwoLightningMoneyInstances_When_Subtracted_Then_ReturnsCorrectDifference()
    {
        // Given
        var money1 = new LightningMoney(3000);
        var money2 = new LightningMoney(1000);

        // When
        var result = money1 - money2;

        // Then
        Assert.Equal(2000UL, result.MilliSatoshi);
    }
    #endregion

    #region ToString
    [Fact]
    public void Given_LightningMoney_When_ToStringCalled_Then_ReturnsCorrectString()
    {
        // Given
        var lightningMoney = new LightningMoney(1_000_000, LightningMoneyUnit.SATOSHI);

        // When
        var result = lightningMoney.ToString();

        // Then
        Assert.Equal("0.01", result);
    }

    [Fact]
    public void Given_LightningMoney_When_ToStringWithTrimCalled_Then_ReturnsTrimmedString()
    {
        // Given
        var lightningMoney = new LightningMoney(1_000_000);

        // When
        var result = lightningMoney.ToString();

        // Then
        Assert.Equal("0.00001", result);
    }

    [Fact]
    public void Given_LightningMoney_When_ToStringWithoutTrimCalled_Then_ReturnsFullString()
    {
        // Given
        var lightningMoney = new LightningMoney(1000000);

        // When
        var result = lightningMoney.ToString(false);

        // Then
        Assert.Equal("0.00001000000", result);
    }
    #endregion

    #region Static Fields
    [Fact]
    public void Given_ZeroMilliSatoshi_When_IsZeroCalled_Then_ReturnsTrue()
    {
        // Given
        var lightningMoney = LightningMoney.Zero;

        // When
        var isZero = lightningMoney.IsZero;

        // Then
        Assert.True(isZero);
    }
    #endregion

    [Fact]
    public void Given_LightningMoney_When_Compared_Then_ReturnsCorrectComparison()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(2000);

        // When & Then
        Assert.True(money1 < money2);
        Assert.True(money2 > money1);
        Assert.True(money1 <= money2);
        Assert.True(money2 >= money1);
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Given_LightningMoney_When_MinAndMaxCalled_Then_ReturnsCorrectValues()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(2000);

        // When
        var min = LightningMoney.Min(money1, money2);
        var max = LightningMoney.Max(money1, money2);

        // Then
        Assert.Equal(money1, min);
        Assert.Equal(money2, max);
    }

    #region Conversion
    [Fact]
    public void Given_LightningMoney_When_ImplicitConversionCalled_Then_ReturnsCorrectValues()
    {
        // Given
        const ulong MILLI_SATOSHI = 1000;

        // When
        LightningMoney lightningMoney = MILLI_SATOSHI;
        ulong backToMilliSatoshi = lightningMoney;

        // Then
        Assert.Equal(MILLI_SATOSHI, lightningMoney.MilliSatoshi);
        Assert.Equal(MILLI_SATOSHI, backToMilliSatoshi);
    }
    #endregion

    [Fact]
    public void Given_LightningMoney_When_AlmostCalled_Then_ReturnsCorrectResult()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(1005);
        var margin = new LightningMoney(10);

        // When
        var isAlmost = money1.Almost(money2, margin);

        // Then
        Assert.True(isAlmost);
    }

    [Fact]
    public void Given_LightningMoney_When_AlmostCalledWithDecimal_Then_ReturnsCorrectResult()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(1005);
        const decimal MARGIN = 0.01m;

        // When
        var isAlmost = money1.Almost(money2, MARGIN);

        // Then
        Assert.True(isAlmost);
    }

    [Fact]
    public void Given_InvalidLightningMoneyUnit_When_Checked_Then_ThrowsArgumentException()
    {
        // Given
        const LightningMoneyUnit INVALID_UNIT = (LightningMoneyUnit)999;

        // When & Then
        Assert.Throws<ArgumentException>(() => LightningMoney.FromUnit(1, INVALID_UNIT));
    }

    [Fact]
    public void Given_LightningMoney_When_NegateCalled_Then_ThrowsArithmeticException()
    {
        // Given
        var lightningMoney = new LightningMoney(1000);

        // When & Then
        Assert.Throws<ArithmeticException>(() => lightningMoney.Negate());
    }

    #region Static Converters
    [Fact]
    public void Given_LightningMoney_When_ConvertedToOtherUnits_Then_ReturnsCorrectValues()
    {
        // Given
        var lightningMoney = new LightningMoney(1_000_000);

        // When
        var inBtc = lightningMoney.ToUnit(LightningMoneyUnit.BTC);
        var inSatoshi = lightningMoney.ToUnit(LightningMoneyUnit.SATOSHI);

        // Then
        Assert.Equal(0.00001m, inBtc);
        Assert.Equal(1000m, inSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_ConvertedToDecimal_Then_ReturnsCorrectValues()
    {
        // Given
        var lightningMoney = new LightningMoney(1_000_000);

        // When
        var inBtc = lightningMoney.ToDecimal(LightningMoneyUnit.BTC);
        var inSatoshi = lightningMoney.ToDecimal(LightningMoneyUnit.SATOSHI);

        // Then
        Assert.Equal(0.00001m, inBtc);
        Assert.Equal(1000m, inSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_CoinsCalled_Then_ReturnsCorrectValue()
    {
        // Given
        const decimal COINS = 1.5m;

        // When
        var result = LightningMoney.Coins(COINS);

        // Then
        Assert.Equal(150_000_000_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_BitsCalled_Then_ReturnsCorrectValue()
    {
        // Given
        const decimal BITS = 1.5m;

        // When
        var result = LightningMoney.Bits(BITS);

        // Then
        Assert.Equal(1_500_000_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_CentsCalled_Then_ReturnsCorrectValue()
    {
        // Given
        const decimal CENTS = 1.5m;

        // When
        var result = LightningMoney.Cents(CENTS);

        // Then
        Assert.Equal(1_500_000_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_SatoshisCalledWithDecimal_Then_ReturnsCorrectValue()
    {
        // Given
        const decimal SATOSHIS = 1.5m;

        // When
        var result = LightningMoney.Satoshis(SATOSHIS);

        // Then
        Assert.Equal(1_500UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_SatoshisCalledWithLong_Then_ReturnsCorrectValue()
    {
        // Given
        const long SATOSHIS = 1_000;

        // When
        var result = LightningMoney.Satoshis(SATOSHIS);

        // Then
        Assert.Equal(1_000_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_SatoshisCalledWithULong_Then_ReturnsCorrectValue()
    {
        // Given
        const ulong SATOSHIS = 1_000;

        // When
        var result = LightningMoney.Satoshis(SATOSHIS);

        // Then
        Assert.Equal(1_000_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_MilliSatoshisCalledWithLong_Then_ReturnsCorrectValue()
    {
        // Given
        const long MILLI_SATOSHIS = 1_000;

        // When
        var result = LightningMoney.MilliSatoshis(MILLI_SATOSHIS);

        // Then
        Assert.Equal(1_000UL, result.MilliSatoshi);
    }

    [Fact]
    public void Given_LightningMoney_When_MilliSatoshisCalledWithULong_Then_ReturnsCorrectValue()
    {
        // Given
        const ulong MILLI_SATOSHIS = 1_000;

        // When
        var result = LightningMoney.MilliSatoshis(MILLI_SATOSHIS);

        // Then
        Assert.Equal(1_000UL, result.MilliSatoshi);
    }
    #endregion

    #region Equality
    [Fact]
    public void Given_TwoEqualLightningMoneyInstances_When_EqualsCalled_Then_ReturnsTrue()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(1000);

        // When
        var areEqual = money1.Equals(money2);

        // Then
        Assert.True(areEqual);
    }

    [Fact]
    public void Given_TwoDifferentLightningMoneyInstances_When_EqualsCalled_Then_ReturnsFalse()
    {
        // Given
        var money1 = new LightningMoney(1000);
        var money2 = new LightningMoney(2000);

        // When
        var areEqual = money1.Equals(money2);

        // Then
        Assert.False(areEqual);
    }

    [Fact]
    public void Given_LightningMoneyAndNull_When_EqualsCalled_Then_ReturnsFalse()
    {
        // Given
        var money = new LightningMoney(1000);

        // When
        var areEqual = money.Equals(null);

        // Then
        Assert.False(areEqual);
    }

    [Fact]
    public void Given_LightningMoneyAndMoney_When_IsCompatibleCalled_Then_ReturnsTrue()
    {
        // Given
        IMoney money1 = new LightningMoney(1000);
        IMoney money2 = new LightningMoney(2000);

        // When
        var isCompatible = money1.IsCompatible(money2);

        // Then
        Assert.True(isCompatible);
    }

    [Fact]
    public void Given_LightningMoneyAndMoney_When_IsCompatibleCalled_Then_ThrowsArgumentNullException()
    {
        // Given
        IMoney money1 = new LightningMoney(1000);
        IMoney money2 = new Money(2000L);

        // When
        var result = money1.IsCompatible(money2);

        // Then
        Assert.False(result);
    }
    #endregion
}