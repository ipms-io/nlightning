namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Utils;

public class PingMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsPingMessageWithCorrectPayload()
    {
        // Arrange
        var expectedPayload = new PingPayload
        {
            BytesLength = 1,
            Ignored = new byte[2],
            NumPongBytes = 3
        };
        var stream = new MemoryStream("000300010000".GetBytes());

        // Act
        var pingMessage = await PingMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
        Assert.Equal(expectedPayload.NumPongBytes, pingMessage.Payload.NumPongBytes);
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new PingMessage(new PingPayload
        {
            BytesLength = 1,
            Ignored = new byte[2],
            NumPongBytes = 3
        });
        var stream = new MemoryStream();
        var expectedBytes = "0012000300010000".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}