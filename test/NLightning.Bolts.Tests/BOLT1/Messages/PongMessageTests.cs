using NLightning.Bolts.Tests.Utils;
using NLightning.Common.Messages;
using NLightning.Common.Messages.Payloads;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

public class PongMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsPongMessageWithCorrectPayload()
    {
        // Arrange
        var expectedPayload = new PongPayload(10);
        var stream = new MemoryStream("000A00000000000000000000".ToByteArray());

        // Act
        var pingMessage = await PongMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedPayload.BytesLength, pingMessage.Payload.BytesLength);
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new PongMessage(10);
        var stream = new MemoryStream();
        var expectedBytes = "0013000A00000000000000000000".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}