namespace NLightning.Domain.Tests.Protocol.Models;

using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.ValueObjects;

public class TlvStreamTests
{
    [Fact]
    public void Given_ValidTlv_When_AddedToStream_Then_ExistsInStream()
    {
        // Given
        var tlvStream = new TlvStream();
        var tlv = new BaseTlv(new BigSize(1), [0x01, 0x02]);

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
        var tlv = new BaseTlv(new BigSize(1), [0x01, 0x02]);

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
        var tlv1 = new BaseTlv(new BigSize(1), [0x01]);
        var tlv2 = new BaseTlv(new BigSize(2), [0x02]);

        // When
        tlvStream.Add(tlv1, tlv2);

        // Then
        Assert.True(tlvStream.TryGetTlv(tlv1.Type, out var retrievedTlv1));
        Assert.True(tlvStream.TryGetTlv(tlv2.Type, out var retrievedTlv2));
        Assert.Equal(tlv1, retrievedTlv1);
        Assert.Equal(tlv2, retrievedTlv2);
    }
}