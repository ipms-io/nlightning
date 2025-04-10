using System.Runtime.Serialization;

namespace NLightning.Common.Tests.Types;

using Common.Types;

public class TlvStreamTests
{
    [Fact]
    public void Given_ValidTlv_When_AddedToStream_Then_ExistsInStream()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv = new Tlv(new BigSize(1), [0x01, 0x02]);

        // When
        tlvStream.Add(tlv);

        // Then
        Assert.True(tlvStream.TryGetTlv(tlv.Type, out var retrievedTlv));
        Assert.Equal(tlv, retrievedTlv);
    }

    [Fact]
    public void Given_DuplicateTlv_When_AddedToStream_Then_ThrowsArgumentException()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv = new Tlv(new BigSize(1), [0x01, 0x02]);

        // When
        tlvStream.Add(tlv);

        // Then
        Assert.Throws<ArgumentException>(() => tlvStream.Add(tlv));
    }

    [Fact]
    public void Given_MultipleTlvs_When_AddedToStream_Then_AllExistInStream()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv1 = new Tlv(new BigSize(1), [0x01]);
        var tlv2 = new Tlv(new BigSize(2), [0x02]);

        // When
        tlvStream.Add(tlv1, tlv2);

        // Then
        Assert.True(tlvStream.TryGetTlv(tlv1.Type, out var retrievedTlv1));
        Assert.True(tlvStream.TryGetTlv(tlv2.Type, out var retrievedTlv2));
        Assert.Equal(tlv1, retrievedTlv1);
        Assert.Equal(tlv2, retrievedTlv2);
    }

    [Fact]
    public async Task Given_TlvStream_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv1 = new Tlv(new BigSize(1), [0x01]);
        var tlv2 = new Tlv(new BigSize(2), [0x02]);
        tlvStream.Add(tlv1, tlv2);

        using var memoryStream = new MemoryStream();

        // When
        await tlvStream.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedTlvStream = await TlvStream.DeserializeAsync(memoryStream);

        // Then
        Assert.NotNull(deserializedTlvStream);
        Assert.True(deserializedTlvStream!.TryGetTlv(tlv1.Type, out var retrievedTlv1));
        Assert.True(deserializedTlvStream.TryGetTlv(tlv2.Type, out var retrievedTlv2));
        Assert.Equal(tlv1, retrievedTlv1);
        Assert.Equal(tlv2, retrievedTlv2);
    }

    [Fact]
    public async Task Given_EmptyStream_When_Deserialized_Then_ReturnsNull()
    {
        // Given
        using var memoryStream = new MemoryStream();

        // When
        var deserializedTlvStream = await TlvStream.DeserializeAsync(memoryStream);

        // Then
        Assert.Null(deserializedTlvStream);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsSerializationException()
    {
        // Given
        using var memoryStream = new MemoryStream([0x00]);

        // When & Then
        await Assert.ThrowsAsync<SerializationException>(async () => await TlvStream.DeserializeAsync(memoryStream));
    }
}