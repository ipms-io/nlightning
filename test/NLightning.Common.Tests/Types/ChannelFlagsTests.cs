namespace NLightning.Common.Tests.Types;

using Common.Types;

public class ChannelFlagsTests
{
    [Fact]
    public void Given_ByteValue_When_ChannelFlagsCreated_Then_PropertiesAreCorrect()
    {
        // Given
        const byte VALUE = 1;

        // When
        var channelFlags = new ChannelFlags(VALUE);

        // Then
        Assert.True(channelFlags.ANNOUNCE_CHANNEL);
        Assert.Equal(VALUE, (byte)channelFlags);
    }

    [Fact]
    public void Given_BoolValue_When_ChannelFlagsCreated_Then_PropertiesAreCorrect()
    {
        // Given
        const bool ANNOUNCE_CHANNEL = true;

        // When
        var channelFlags = new ChannelFlags(ANNOUNCE_CHANNEL);

        // Then
        Assert.True(channelFlags.ANNOUNCE_CHANNEL);
        Assert.Equal((byte)1, (byte)channelFlags);
    }

    [Fact]
    public async Task Given_ChannelFlags_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var channelFlags = new ChannelFlags(1);

        using var memoryStream = new MemoryStream();

        // When
        await channelFlags.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedChannelFlags = await ChannelFlags.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(channelFlags, deserializedChannelFlags);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsEndOfStreamException()
    {
        // Given
        using var memoryStream = new MemoryStream([]); // Empty stream

        // When & Then
        await Assert.ThrowsAsync<EndOfStreamException>(async () => await ChannelFlags.DeserializeAsync(memoryStream));
    }

    [Fact]
    public void Given_ChannelFlags_When_ImplicitlyConvertedToByte_Then_ReturnsCorrectValue()
    {
        // Given
        var channelFlags = new ChannelFlags(1);

        // When
        byte byteValue = channelFlags;

        // Then
        Assert.Equal((byte)1, byteValue);
    }

    [Fact]
    public void Given_Byte_When_ImplicitlyConvertedToChannelFlags_Then_ReturnsCorrectChannelFlags()
    {
        // Given
        const byte VALUE = 1;

        // When
        ChannelFlags channelFlags = VALUE;

        // Then
        Assert.True(channelFlags.ANNOUNCE_CHANNEL);
    }

    [Fact]
    public void Given_ChannelFlags_When_ImplicitlyConvertedToByteArray_Then_ReturnsCorrectValue()
    {
        // Given
        var channelFlags = new ChannelFlags(1);

        // When
        byte[] byteArray = channelFlags;

        // Then
        Assert.Equal(new byte[] { 1 }, byteArray);
    }

    [Fact]
    public void Given_ByteArray_When_ImplicitlyConvertedToChannelFlags_Then_ReturnsCorrectChannelFlags()
    {
        // Given
        byte[] value = [1];

        // When
        ChannelFlags channelFlags = value;

        // Then
        Assert.True(channelFlags.ANNOUNCE_CHANNEL);
    }
}