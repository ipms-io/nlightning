namespace NLightning.Domain.Tests.ValueObjects;

using Domain.ValueObjects;
using Enums;

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
        Assert.True(channelFlags.AnnounceChannel);
        Assert.Equal(VALUE, (byte)channelFlags);
    }

    [Fact]
    public void Given_BoolValue_When_ChannelFlagsCreated_Then_PropertiesAreCorrect()
    {
        // Given
        const ChannelFlag ANNOUNCE_CHANNEL = ChannelFlag.AnnounceChannel;

        // When
        var channelFlags = new ChannelFlags(ANNOUNCE_CHANNEL);

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
        const byte VALUE = 1;

        // When
        ChannelFlags channelFlags = VALUE;

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