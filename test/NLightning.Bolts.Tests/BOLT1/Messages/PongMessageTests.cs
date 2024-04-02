using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;

public class PongMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_IsCalled_Then_ReturnsPongMessageWithCorrectPayload()
    {
        // Given
        var expectedPayload = new PongPayload(10);
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // When
        var pingMessage = await PongMessage.DeserializeAsync(stream);

        // Then
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Given
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // When & Then
        await Assert.ThrowsAsync<MessageSerializationException>(() => PongMessage.DeserializeAsync(invalidStream));
    }
}