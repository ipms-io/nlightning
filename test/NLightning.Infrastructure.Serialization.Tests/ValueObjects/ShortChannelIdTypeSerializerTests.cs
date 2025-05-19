namespace NLightning.Infrastructure.Serialization.Tests.ValueObjects;

using Domain.ValueObjects;
using Infrastructure.Serialization.ValueObjects;

public class ShortChannelIdTypeSerializerTests
{
    private const uint EXPECTED_BLOCK_HEIGHT = 870127;
    private const uint EXPECTED_TX_INDEX = 1237;
    private const ushort EXPECTED_OUTPUT_INDEX = 1;

    [Fact]
    public async Task Given_ValidShortChannelId_When_SerializedAndDeserialized_Then_PropertiesRemainTheSame()
    {
        // Given
        var shortChannelIdTypeSerializer = new ShortChannelIdTypeSerializer();
        var original = new ShortChannelId(EXPECTED_BLOCK_HEIGHT, EXPECTED_TX_INDEX, EXPECTED_OUTPUT_INDEX);
        using var ms = new MemoryStream();

        // When
        await shortChannelIdTypeSerializer.SerializeAsync(original, ms);
        ms.Position = 0; // reset stream position
        var deserialized = await shortChannelIdTypeSerializer.DeserializeAsync(ms);

        // Then
        Assert.Equal(original, deserialized);
    }
}