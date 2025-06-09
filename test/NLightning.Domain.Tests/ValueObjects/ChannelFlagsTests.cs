using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Tests.ValueObjects;

using Enums;

public class ChannelFlagsTests
{
    [Fact]
    public void Given_ByteValue_When_ChannelFlagsCreated_Then_PropertiesAreCorrect()
    {
        // Given
        const byte value = 1;

        // When
        var channelFlags = new ChannelFlags(value);

        // Then
        Assert.True(channelFlags.AnnounceChannel);
        Assert.Equal(value, (byte)channelFlags);
    }

    [Fact]
    public void Given_BoolValue_When_ChannelFlagsCreated_Then_PropertiesAreCorrect()
    {
        // Given
        const ChannelFlag announceChannel = ChannelFlag.AnnounceChannel;

        // When
        var channelFlags = new ChannelFlags(announceChannel);

        // Then
        Assert.True(channelFlags.AnnounceChannel);
        Assert.Equal((byte)1, (byte)channelFlags);
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
        const byte value = 1;

        // When
        ChannelFlags channelFlags = value;

        // Then
        Assert.True(channelFlags.AnnounceChannel);
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
        Assert.True(channelFlags.AnnounceChannel);
    }
}