using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;

public class PingMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_IsCalled_Then_ReturnsPingMessageWithCorrectPayload()
    {
        // Given
        var expectedPayload = new PingPayload();
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // When
        var pingMessage = await PingMessage.DeserializeAsync(stream);

        // Then
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
        Assert.Equal(expectedPayload.NumPongBytes, pingMessage.Payload.NumPongBytes);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Given
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // When & Then
        await Assert.ThrowsAsync<MessageSerializationException>(() => PingMessage.DeserializeAsync(invalidStream));
    }
}