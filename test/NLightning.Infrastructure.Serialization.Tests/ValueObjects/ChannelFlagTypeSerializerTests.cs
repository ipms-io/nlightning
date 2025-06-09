using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Infrastructure.Serialization.ValueObjects;

public class ChannelFlagTypeSerializerTests
{
    [Fact]
    public async Task Given_ChannelFlags_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var channelFlagsSerializer = new ChannelFlagTypeSerializer();
        var channelFlags = new ChannelFlags(1);

        using var memoryStream = new MemoryStream();

        // When
        await channelFlagsSerializer.SerializeAsync(channelFlags, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedChannelFlags = await channelFlagsSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(channelFlags, deserializedChannelFlags);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsEndOfStreamException()
    {
        // Given
        var channelFlagsSerializer = new ChannelFlagTypeSerializer();
        using var memoryStream = new MemoryStream([]); // Empty stream

        // When & Then
        await Assert.ThrowsAsync<EndOfStreamException>(async () =>
                                                           await channelFlagsSerializer.DeserializeAsync(memoryStream));
    }
}