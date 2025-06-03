namespace NLightning.Domain.Tests.Utils;

using Domain.Utils;

public class BitReaderTests
{
    #region Test Data

    private static readonly byte[] s_sampleBuffer =
    [
        0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01
    ];

    #endregion

    [Fact]
    public void Given_SampleBuffer_When_ReadBitRepeatedly_Then_BitsMatchExpected()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var b0 = reader.ReadBit(); // 0
        var b1 = reader.ReadBit(); // 0
        var b2 = reader.ReadBit(); // 0
        var b3 = reader.ReadBit(); // 1
        var b4 = reader.ReadBit(); // 0
        var b5 = reader.ReadBit(); // 0
        var b6 = reader.ReadBit(); // 1
        var b7 = reader.ReadBit(); // 0

        var b8 = reader.ReadBit(); // 0
        var b9 = reader.ReadBit(); // 0
        var b10 = reader.ReadBit(); // 1
        var b11 = reader.ReadBit(); // 1
        var b12 = reader.ReadBit(); // 0
        var b13 = reader.ReadBit(); // 1
        var b14 = reader.ReadBit(); // 0
        var b15 = reader.ReadBit(); // 0

        // Then
        Assert.False(b0);
        Assert.False(b1);
        Assert.False(b2);
        Assert.True(b3);
        Assert.False(b4);
        Assert.False(b5);
        Assert.True(b6);
        Assert.False(b7);

        Assert.False(b8);
        Assert.False(b9);
        Assert.True(b10);
        Assert.True(b11);
        Assert.False(b12);
        Assert.True(b13);
        Assert.False(b14);
        Assert.False(b15);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadByteFromBits_Then_ExtractedByteIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var b1 = reader.ReadByteFromBits(8);

        // Then
        Assert.Equal(0x12, b1);

        // Next read => next 8 bits => 0x34
        var b2 = reader.ReadByteFromBits(8);
        Assert.Equal(0x34, b2);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadByteFromBitsPartial_Then_ExtractedByteMatches()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var highNibble = reader.ReadByteFromBits(4);
        var lowNibble = reader.ReadByteFromBits(4);

        // Then
        Assert.Equal(0x1, highNibble);
        Assert.Equal(0x2, lowNibble);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadInt16FromBitsBigEndian_Then_ValueIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var val = reader.ReadInt16FromBits(16, bigEndian: true);

        // Then
        Assert.Equal(0x1234, (ushort)val);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadInt16FromBitsLittleEndian_Then_ValueIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var val = reader.ReadInt16FromBits(16, bigEndian: false);

        // Then
        Assert.Equal(0x3412, (ushort)val);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadUInt16FromBits_Then_ValueIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        reader.SkipBits(8);
        var val = reader.ReadUInt16FromBits(16, bigEndian: true);

        // Then
        Assert.Equal(0x3456, val);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadInt32FromBits_Then_ValueIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var val = reader.ReadInt32FromBits(32, bigEndian: true);

        // Then
        Assert.Equal(0x12345678, val);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadInt64FromBits_Then_ValueIsCorrect()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var val = reader.ReadInt64FromBits(64, bigEndian: true);

        // Then
        Assert.Equal(0x12345678ABCDEF01, val);
    }

    [Fact]
    public void Given_SampleBuffer_When_HasMoreBitsIsCalled_Then_ReturnsTrueIfEnoughBitsRemain()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);

        // When
        var canRead50 = reader.HasMoreBits(50);
        reader.SkipBits(60);
        var canRead5 = reader.HasMoreBits(5);
        var canRead4 = reader.HasMoreBits(4);

        // Then
        Assert.True(canRead50);
        Assert.False(canRead5);
        Assert.True(canRead4);
    }

    [Fact]
    public void Given_SampleBuffer_When_ReadBeyondAvailableBits_Then_ThrowsException()
    {
        // Given
        var reader = new BitReader(s_sampleBuffer);
        reader.SkipBits(64);

        // When / Then
        Assert.Throws<InvalidOperationException>(() => reader.ReadBit());
    }
}