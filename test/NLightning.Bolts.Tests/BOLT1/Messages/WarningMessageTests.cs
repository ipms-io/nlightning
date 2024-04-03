using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using BOLT1.Mock;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;

public class WarningMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_IsCalled_Then_ReturnsWarningMessageWithCorrectPayload()
    {
        // Arrange
        var expectedPayload = new ErrorPayload("Warning message!");
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // Act
        var errorMessage = await WarningMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(errorMessage);
        Assert.Equal(expectedPayload.ChannelId, errorMessage.Payload.ChannelId);
        Assert.Equal(expectedPayload.Data, errorMessage.Payload.Data);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => WarningMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_StreamReadWarning_When_DeserializeAsync_IsCalled_Then_ThrowsIOException()
    {
        // Arrange
        var brokenStream = new FakeBrokenStream(); // You would need to mock or implement a stream that simulates a read error.

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => WarningMessage.DeserializeAsync(brokenStream));
    }
}