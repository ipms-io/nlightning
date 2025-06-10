namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Domain.Channels.ValueObjects;
using Infrastructure.Serialization.ValueObjects;

public class ShortChannelIdTypeSerializerTests
{
    private const uint ExpectedBlockHeight = 870127;
    private const uint ExpectedTxIndex = 1237;
    private const ushort ExpectedOutputIndex = 1;

    [Fact]
    public async Task Given_ValidShortChannelId_When_SerializedAndDeserialized_Then_PropertiesRemainTheSame()
    {
        // Given
        var shortChannelIdTypeSerializer = new ShortChannelIdTypeSerializer();
        var original = new ShortChannelId(ExpectedBlockHeight, ExpectedTxIndex, ExpectedOutputIndex);
        using var ms = new MemoryStream();

        // When
        await shortChannelIdTypeSerializer.SerializeAsync(original, ms);
        ms.Position = 0; // reset stream position
        var deserialized = await shortChannelIdTypeSerializer.DeserializeAsync(ms);

        // Then
        Assert.Equal(original, deserialized);
    }
}