using System.Runtime.Serialization;

namespace NLightning.Common.Tests.Types;

using Common.Types;

public class TlvTests
{
    [Fact]
    public async Task Given_ValidTlv_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var type = new BigSize(1);
        var value = new byte[] { 0x01, 0x02, 0x03 };
        var tlv = new Tlv(type, value);

        using var memoryStream = new MemoryStream();

        // When
        await tlv.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedTlv = await Tlv.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(tlv.Type, deserializedTlv.Type);
        Assert.Equal(tlv.Length, deserializedTlv.Length);
        Assert.Equal(tlv.Value, deserializedTlv.Value);
    }

    [Fact]
    public async Task Given_EmptyValueTlv_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var type = new BigSize(1);
        var tlv = new Tlv(type);

        using var memoryStream = new MemoryStream();

        // When
        await tlv.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedTlv = await Tlv.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(tlv.Type, deserializedTlv.Type);
        Assert.Equal(tlv.Length, deserializedTlv.Length);
        Assert.Equal(tlv.Value, deserializedTlv.Value);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsSerializationException()
    {
        // Given
        using var memoryStream = new MemoryStream([0x00]);

        // When & Then
        await Assert.ThrowsAsync<SerializationException>(async () => await Tlv.DeserializeAsync(memoryStream));
    }

    [Fact]
    public void Given_TwoTlvsWithSameType_When_Compared_Then_AreEqual()
    {
        // Given
        var type = new BigSize(1);
        var tlv1 = new Tlv(type, [0x01]);
        var tlv2 = new Tlv(type, [0x02]);

        // When
        var areEqual = tlv1.Equals(tlv2);

        // Then
        Assert.True(areEqual);
    }

    [Fact]
    public void Given_TwoTlvsWithDifferentTypes_When_Compared_Then_AreNotEqual()
    {
        // Given
        var tlv1 = new Tlv(new BigSize(1), [0x01]);
        var tlv2 = new Tlv(new BigSize(2), [0x01]);

        // When
        var areEqual = tlv1.Equals(tlv2);

        // Then
        Assert.False(areEqual);
    }

    [Fact]
    public void Given_Tlv_When_HashCodeCalled_Then_ReturnsTypeHashCode()
    {
        // Given
        var type = new BigSize(1);
        var tlv = new Tlv(type, [0x01]);

        // When
        var hashCode = tlv.GetHashCode();

        // Then
        Assert.Equal(type.GetHashCode(), hashCode);
    }
}