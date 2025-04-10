namespace NLightning.Common.Tests.Types;

using Common.Types;

public class ChainHashTests
{
    [Fact]
    public void Given_TwoEqualChainHashes_When_Compared_Then_AreEqual()
    {
        // Given
        var value = new byte[32];
        var chainHash1 = new ChainHash(value);
        var chainHash2 = new ChainHash(value);

        // When & Then
        Assert.True(chainHash1 == chainHash2);
        Assert.False(chainHash1 != chainHash2);
        Assert.True(chainHash1.Equals(chainHash2));
        Assert.True(chainHash2.Equals(chainHash1));
    }

    [Fact]
    public void Given_TwoDifferentChainHashes_When_Compared_Then_AreNotEqual()
    {
        // Given
        var chainHash1 = new ChainHash(new byte[32]);
        var chainHash2 = new ChainHash([1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1
        ]);

        // When & Then
        Assert.False(chainHash1 == chainHash2);
        Assert.True(chainHash1 != chainHash2);
        Assert.False(chainHash1.Equals(chainHash2));
        Assert.False(chainHash2.Equals(chainHash1));
    }

    [Fact]
    public void Given_InvalidByteArray_When_ChainHashCreated_Then_ThrowsArgumentOutOfRangeException()
    {
        // Given
        var invalidValue = new byte[31];

        // When & Then
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChainHash(invalidValue));
    }

    [Fact]
    public async Task Given_ValidChainHash_When_SerializedAndDeserialized_Then_DataIsPreserved()
    {
        // Given
        var value = new byte[32];
        for (var i = 0; i < value.Length; i++) value[i] = (byte)i;
        var chainHash = new ChainHash(value);

        using var memoryStream = new MemoryStream();

        // When
        await chainHash.SerializeAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var deserializedChainHash = await ChainHash.DeserializeAsync(memoryStream);

        // Then
        Assert.Equal(chainHash, deserializedChainHash);
    }

    [Fact]
    public async Task Given_InvalidStream_When_Deserialized_Then_ThrowsEndOfStreamException()
    {
        // Given
        using var memoryStream = new MemoryStream(new byte[16]); // Less than 32 bytes

        // When & Then
        await Assert.ThrowsAsync<EndOfStreamException>(async () => await ChainHash.DeserializeAsync(memoryStream));
    }
}