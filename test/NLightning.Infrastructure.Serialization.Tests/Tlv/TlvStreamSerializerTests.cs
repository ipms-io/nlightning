using System.Runtime.Serialization;
using NLightning.Domain.Money;

namespace NLightning.Infrastructure.Serialization.Tests.Tlv;

using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.ValueObjects;
using Serialization.Tlv;
using Helpers;

public class TlvStreamSerializerTests
{
    private readonly TlvStreamSerializer _tlvStreamSerializer;

    public TlvStreamSerializerTests()
    {
        _tlvStreamSerializer = new TlvStreamSerializer(SerializerHelper.TlvConverterFactory,
                                                       SerializerHelper.TlvSerializer);
    }
    
    [Fact]
    public async Task Given_TlvStream_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv1 = new RequireConfirmedInputsTlv();
        var tlv2 = new FundingOutputContributionTlv(LightningMoney.Satoshis(100_000));
        tlvStream.Add(tlv1, tlv2);

        using var memoryStream = new MemoryStream();

        // When
        await _tlvStreamSerializer.SerializeAsync(tlvStream, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedTlvStream = await _tlvStreamSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.NotNull(deserializedTlvStream);
        Assert.True(deserializedTlvStream.TryGetTlv(tlv1.Type, out var retrievedTlv1));
        Assert.NotNull(retrievedTlv1);
        Assert.Equal(tlv1.Type, retrievedTlv1.Type);
        Assert.True(deserializedTlvStream.TryGetTlv(tlv2.Type, out var retrievedTlv2));
        Assert.NotNull(retrievedTlv2);
        Assert.Equal(tlv2.Type, retrievedTlv2.Type);
    }

    [Fact]
    public async Task Given_EmptyStream_When_Deserialized_Then_ReturnsNull()
    {
        // Given
        using var memoryStream = new MemoryStream();

        // When
        var deserializedTlvStream = await _tlvStreamSerializer.DeserializeAsync(memoryStream);

        // Then
        Assert.Null(deserializedTlvStream);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsSerializationException()
    {
        // Given
        using var memoryStream = new MemoryStream([0x00]);

        // When & Then
        await Assert.ThrowsAsync<SerializationException>(async () => await _tlvStreamSerializer.DeserializeAsync(memoryStream));
    }
}