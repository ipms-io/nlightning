namespace NLightning.Common.Tests.Utils;

using Common.Utils;

public class BitWriterTests
{
    [Fact]
    public void Given_TotalBits_When_ConstructorCalled_Then_BufferCreatedAndNoError()
    {
        // Given
        const int TOTAL_BITS = 16;

        // When
        var writer = new BitWriter(TOTAL_BITS);

        // Then
        Assert.Equal(TOTAL_BITS, writer.TotalBits);
        // Just ensure no exceptions. 
    }

    [Fact]
    public void Given_BitWriter_When_WriteBitRepeatedly_Then_BytesAreSetCorrectly()
    {
        // Given
        // We'll start with 1 byte (8 bits).
        var writer = new BitWriter(8);

        // When
        // Write "0,0,1,1,0,1,0,1" => binary => 0x35
        writer.WriteBit(false);
        writer.WriteBit(false);
        writer.WriteBit(true);
        writer.WriteBit(true);
        writer.WriteBit(false);
        writer.WriteBit(true);
        writer.WriteBit(false);
        writer.WriteBit(true);

        var output = writer.ToArray();

        // Then
        Assert.Single(output);
        // 0x35 => 00110101 in binary
        // bit order: the first bit written is the top bit (bit 7).
        // The actual arrangement is a bit tricky: 
        //   - The code sets the bit (1 << (7 - bitIndex)).
        // The sequence above becomes (0,0,1,1,0,1,0,1) => 0x35
        Assert.Equal(0x35, output[0]);
    }

    [Fact]
    public void Given_BitWriter_When_WriteBitsWithFullByte_Then_DataIsWritten()
    {
        // Given
        var writer = new BitWriter(16); // enough to store 2 bytes
        var data = new byte[] { 0xAB, 0xCD };

        // When
        // Write 16 bits => should store 0xAB, then 0xCD
        writer.WriteBits(data, 16);

        var output = writer.ToArray();

        // Then
        Assert.Equal(2, output.Length);
        Assert.Equal(0xAB, output[0]);
        Assert.Equal(0xCD, output[1]);
    }

    [Fact]
    public void Given_BitWriter_When_WritePartialBits_Then_DataIsPlacedAtCorrectOffset()
    {
        // Given
        // We'll create a 2-byte buffer => 16 bits total
        var writer = new BitWriter(16);

        // We'll write the top 4 bits as 0xC (1100), then next 4 bits as 0x3 (0011)
        // So the first byte becomes 0xC3 => 1100 0011
        // Then next 8 bits => 0xFF 
        // Overall => [0xC3, 0xFF]
        // We'll test that logic.

        // When
        // first 4 bits
        var topNibble = new byte[] { 0xC0 }; // 1100 0000 but only top nibble matters if we write 4 bits
        writer.WriteBits(topNibble, 4);

        // next 4 bits
        var bottomNibble = new byte[] { 0x30 }; // 0011 0000 
        writer.WriteBits(bottomNibble, 4);

        // next 8 bits
        var fullByte = new byte[] { 0xFF };
        writer.WriteBits(fullByte, 8);

        var output = writer.ToArray();

        // Then
        Assert.Equal(2, output.Length);
        // 1100 0011 => 0xC3
        // 1111 1111 => 0xFF
        Assert.Equal(0xC3, output[0]);
        Assert.Equal(0xFF, output[1]);
    }

    [Fact]
    public void Given_BitWriter_When_GrowByBitsCalled_Then_BufferIsResized()
    {
        // Given
        // Start with 8 bits => 1 byte
        var writer = new BitWriter(8);
        Assert.Equal(8, writer.TotalBits);

        // Write 8 bits => fill up
        writer.WriteBits([0xAB], 8);

        // Next we want to write an additional 16 bits => we must grow
        writer.GrowByBits(16);

        // When
        // Now we can write 16 more bits
        writer.WriteBits([0xCD, 0xEF], 16);

        var output = writer.ToArray();

        // Then
        // We expect the array to have 3 bytes => first is 0xAB, next are 0xCD, 0xEF
        Assert.Equal(3, output.Length);
        Assert.Equal(0xAB, output[0]);
        Assert.Equal(0xCD, output[1]);
        Assert.Equal(0xEF, output[2]);
    }

    [Fact]
    public void Given_BitWriter_When_ExceedingTotalBits_Then_Throws()
    {
        // Given
        var writer = new BitWriter(8);

        // We can write 8 bits
        writer.WriteBits([0xAA], 8);

        // When / Then
        // Next bit => should cause an exception
        Assert.Throws<InvalidOperationException>(() => writer.WriteBit(true));
    }

    [Fact]
    public void Given_BitWriter_When_WriteInt16AsBitsInBigEndian_Then_OutputIsCorrect()
    {
        // Given
        // We'll write 16 bits total
        var writer = new BitWriter(16);
        short val = 0x1234; // big-endian => stored as 0x12, then 0x34?

        // When
        writer.WriteInt16AsBits(val, 16, bigEndian: true);

        var output = writer.ToArray();

        // Then
        Assert.Equal(2, output.Length);
        // If the method works as typical big-endian, we'd see 0x12 0x34
        // But the code might do something else due to the bit shifting logic.
        // We'll just verify the raw bytes are consistent with typical big-endian notion.
        Assert.Equal(0x12, output[0]);
        Assert.Equal(0x34, output[1]);
    }

    [Fact]
    public void Given_BitWriter_When_WriteInt32AsBits_Then_OutputCanBeVerified()
    {
        // Given
        // We'll write 32 bits
        var writer = new BitWriter(32);
        const int VAL = 0x00ABCD12;

        // When
        writer.WriteInt32AsBits(VAL, 32, bigEndian: true);

        var outArr = writer.ToArray();

        // Then
        Assert.Equal(4, outArr.Length);
        // Typical big-endian => 0xAB 0xCD 0x12
        Assert.True(outArr.SequenceEqual(new byte[] { 0x00, 0xAB, 0xCD, 0x12 }));
    }

    [Fact]
    public void Given_BitWriter_When_HasMoreBitsIsCalled_Then_CorrectlyShowsRemainingCapacity()
    {
        // Given
        var writer = new BitWriter(16);

        // When
        var canWrite15 = writer.HasMoreBits(15);
        var canWrite16 = writer.HasMoreBits(16);
        var canWrite17 = writer.HasMoreBits(17);

        // Then
        Assert.True(canWrite15);
        Assert.True(canWrite16);
        Assert.False(canWrite17);
    }

    [Fact]
    public void Given_BitWriter_When_ToArrayCalled_Then_ReturnsSnapshotOfBuffer()
    {
        // Given
        var writer = new BitWriter(16);
        writer.WriteBits([0xAB], 8);
        writer.WriteBits([0xCD], 8);

        // When
        var array = writer.ToArray();

        // Then
        // We expect 2 bytes => 0xAB, 0xCD
        Assert.Equal(new byte[] { 0xAB, 0xCD }, array);
    }
}