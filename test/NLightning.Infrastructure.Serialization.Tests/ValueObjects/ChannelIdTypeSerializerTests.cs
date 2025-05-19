namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Domain.ValueObjects;
using Infrastructure.Serialization.ValueObjects;

public class ChannelIdTypeSerializerTests
{
    [Fact]
    public async Task Given_ValidChannelId_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var channelIdTypeSerializer = new ChannelIdTypeSerializer();
        var value = new byte[32];
        for (var i = 0; i < value.Length; i++) value[i] = (byte)i;
        var channelId = new ChannelId(value);

        using var memoryStream = new MemoryStream();

        // When
        await channelIdTypeSerializer.SerializeAsync(channelId, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedChannelId = await channelIdTypeSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(channelId, deserializedChannelId);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsEndOfStreamException()
    {
        // Given
        var channelIdTypeSerializer = new ChannelIdTypeSerializer();
        using var memoryStream = new MemoryStream(new byte[16]); // Less than 32 bytes

        // When & Then
        await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            await channelIdTypeSerializer.DeserializeAsync(memoryStream));
    }
}