using NLightning.Common.BitUtils;

namespace NLightning.Common.Tests.BitUtils;

public class EndianBitConverterTests
{
    #region ULong
    [Fact]
    public void Given_UlongValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const ulong VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_UlongValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const ulong VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_UlongValue_When_ConvertedToLittleEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const ulong VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_UlongValue_When_ConvertedToLittleEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const ulong VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUlong_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToUInt64BigEndian(bytes);

        // Then
        Assert.Equal(0x0123UL, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUlongPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToUInt64BigEndian(bytes, true);

        // Then
        Assert.Equal(0x0123UL, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUlong_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt64LittleEndian(bytes);

        // Then
        Assert.Equal(0x0123UL, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUlongPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt64LittleEndian(bytes, true);

        // Then
        Assert.Equal(0x0123UL, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt64BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt64BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt64LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt64LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt64BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt64BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt64LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt64LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt64BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const ulong VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt64LittleEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const ulong VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion

    #region Long
    [Fact]
    public void Given_LongValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const long VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_LongValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const long VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_LongValue_When_ConvertedToLittleEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const long VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_LongValue_When_ConvertedToLittleEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const long VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToLong_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToInt64BigEndian(bytes);

        // Then
        Assert.Equal(0x0123L, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToLongPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToInt64BigEndian(bytes, true);

        // Then
        Assert.Equal(0x0123L, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToLong_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToInt64LittleEndian(bytes);

        // Then
        Assert.Equal(0x0123L, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToLongPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToInt64LittleEndian(bytes, true);

        // Then
        Assert.Equal(0x0123L, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt64BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt64BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt64LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt64LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt64BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt64BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt64LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt64LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToInt64BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const long VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToInt64LittleEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const long VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion

    #region UInt
    [Fact]
    public void Given_UintValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const uint VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x00, 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_UintValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const uint VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_UintValue_When_ConvertedToLittleEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const uint VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_UintValue_When_ConvertedToLittleEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const uint VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01, 0x00, 0x00 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUint_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x00, 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToUInt32BigEndian(bytes);

        // Then
        Assert.Equal(0x0123U, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUintPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToUInt32BigEndian(bytes, true);

        // Then
        Assert.Equal(0x0123U, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUint_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt32LittleEndian(bytes);

        // Then
        Assert.Equal(0x0123U, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUintPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt32LittleEndian(bytes, true);

        // Then
        Assert.Equal(0x0123U, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt32BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt32BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt32LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt32LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt32BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt32BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt32LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt32LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt32BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const uint VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt32LittleEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const uint VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion

    #region Int
    [Fact]
    public void Given_IntValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const int VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x00, 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_IntValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const int VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x23 }, result);
    }

    [Fact]
    public void Given_IntValue_When_ConvertedToLittleEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const int VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01 }, result);
    }

    [Fact]
    public void Given_IntValue_When_ConvertedToLittleEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const int VALUE = 0x0123;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x23, 0x01 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToInt_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x00, 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToInt32BigEndian(bytes);

        // Then
        Assert.Equal(0x0123, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToIntPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When
        var result = EndianBitConverter.ToInt32BigEndian(bytes, true);

        // Then
        Assert.Equal(0x0123, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToInt_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToInt32LittleEndian(bytes);

        // Then
        Assert.Equal(0x0123, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToIntPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x23, 0x01, 0x00, 0x00 };

        // When
        var result = EndianBitConverter.ToInt32LittleEndian(bytes, true);

        // Then
        Assert.Equal(0x0123, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt32BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt32BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt32LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x23 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt32LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt32BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt32BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt32LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt32LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToInt32BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const int VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToInt32LittleEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const int VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion

    #region UShort
    [Fact]
    public void Given_UshortValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const ushort VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x01 }, result);
    }

    [Fact]
    public void Given_UshortValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const ushort VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01 }, result);
    }

    [Fact]
    public void Given_UshortValue_When_ConvertedToLittleEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const ushort VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x00 }, result);
    }

    [Fact]
    public void Given_UshortValue_When_ConvertedToLittleEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const ushort VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01, 0x00 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUshort_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x01 };

        // When
        var result = EndianBitConverter.ToUInt16BigEndian(bytes);

        // Then
        Assert.Equal((ushort)0x01, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToUshortPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When
        var result = EndianBitConverter.ToUInt16BigEndian(bytes, true);

        // Then
        Assert.Equal((ushort)0x01, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUshort_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt16LittleEndian(bytes);

        // Then
        Assert.Equal((ushort)0x01, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToUshortPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x00 };

        // When
        var result = EndianBitConverter.ToUInt16LittleEndian(bytes, true);

        // Then
        Assert.Equal((ushort)0x01, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt16BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt16BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToUInt16LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToUInt16LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt16BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt16BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToUInt16LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToUInt16LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt16BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const ushort VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToUInt16LittleEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const ushort VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesLittleEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion

    #region Short
    [Fact]
    public void Given_ShortValue_When_ConvertedToBigEndianBytes_Then_ReturnsCorrectByteArray()
    {
        // Given
        const short VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE);

        // Then
        Assert.Equal(new byte[] { 0x00, 0x01 }, result);
    }

    [Fact]
    public void Given_ShortValue_When_ConvertedToBigEndianBytesWithTrim_Then_ReturnsFullByteArray()
    {
        // Given
        const short VALUE = 0x01;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x01 }, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToShort_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x00, 0x01 };

        // When
        var result = EndianBitConverter.ToInt16BigEndian(bytes);

        // Then
        Assert.Equal((short)0x01, result);
    }

    [Fact]
    public void Given_BigEndianBytes_When_ConvertedToShortPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When
        var result = EndianBitConverter.ToInt16BigEndian(bytes, true);

        // Then
        Assert.Equal((short)0x01, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToShort_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x00 };

        // When
        var result = EndianBitConverter.ToInt16LittleEndian(bytes);

        // Then
        Assert.Equal((short)0x01, result);
    }

    [Fact]
    public void Given_LittleEndianBytes_When_ConvertedToShortPadded_Then_ReturnsCorrectValue()
    {
        // Given
        var bytes = new byte[] { 0x01, 0x00 };

        // When
        var result = EndianBitConverter.ToInt16LittleEndian(bytes, true);

        // Then
        Assert.Equal((short)0x01, result);
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt16BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt16BigEndian(bytes));
    }

    [Fact]
    public void Given_ShortByteArray_When_NotPaddedToInt16LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = new byte[] { 0x01 };

        // When & Then
        Assert.Throws<ArgumentException>(() => EndianBitConverter.ToInt16LittleEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt16BigEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt16BigEndian(bytes));
    }

    [Fact]
    public void Given_EmptyByteArray_When_ConvertedToInt16LittleEndian_Then_ThrowsArgumentException()
    {
        // Given
        var bytes = Array.Empty<byte>();

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => EndianBitConverter.ToInt16LittleEndian(bytes));
    }

    [Fact]
    public void Given_AllZeroBytes_When_ConvertedToInt16BigEndianTrimmed_Then_ReturnsSingleZeroByte()
    {
        // Given
        const short VALUE = 0;

        // When
        var result = EndianBitConverter.GetBytesBigEndian(VALUE, true);

        // Then
        Assert.Equal(new byte[] { 0x00 }, result);
    }
    #endregion
}