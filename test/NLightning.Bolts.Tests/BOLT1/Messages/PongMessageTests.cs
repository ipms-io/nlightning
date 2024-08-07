using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Exceptions;

public class PongMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_IsCalled_Then_ReturnsPongMessageWithCorrectPayload()
    {
        // Arrange
        var expectedPayload = new PongPayload(10);
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // Act
        var pingMessage = await PongMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => PongMessage.DeserializeAsync(invalidStream));
    }
}