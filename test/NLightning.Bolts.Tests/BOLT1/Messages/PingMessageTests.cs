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
        // Arrange
        var expectedPayload = new PingPayload();
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // Act
        var pingMessage = await PingMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
        Assert.Equal(expectedPayload.NumPongBytes, pingMessage.Payload.NumPongBytes);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => PingMessage.DeserializeAsync(invalidStream));
    }
}