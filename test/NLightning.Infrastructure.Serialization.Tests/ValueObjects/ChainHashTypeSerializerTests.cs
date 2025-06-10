using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Infrastructure.Serialization.ValueObjects;

public class ChainHashTypeSerializerTests
{
    [Fact]
    public async Task Given_ValidChainHash_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var chainHashSerializer = new ChainHashTypeSerializer();
        var value = new byte[32];
        for (var i = 0; i < value.Length; i++) value[i] = (byte)i;
        var chainHash = new ChainHash(value);

        using var memoryStream = new MemoryStream();

        // When
        await chainHashSerializer.SerializeAsync(chainHash, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedChainHash = await chainHashSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(chainHash, deserializedChainHash);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsEndOfStreamException()
    {
        // Given
        var chainHashSerializer = new ChainHashTypeSerializer();
        using var memoryStream = new MemoryStream(new byte[16]); // Less than 32 bytes

        // When & Then
        await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            await chainHashSerializer.DeserializeAsync(memoryStream));
    }
}