namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class TxCompleteMessageTests
{
    [Fact]
    public async Task DeserializeAsync_GivenValidStream_ReturnsTxCompleteMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var stream = new MemoryStream("0x0000000000000000000000000000000000000000000000000000000000000000".ToByteArray());

        // Act
        var message = await TxCompleteMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
    }

    [Fact]
    public async Task DeserializeAsync_GivenInvalidStreamContent_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxCompleteMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task SerializeAsync_GivenValidPayload_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var message = new TxCompleteMessage(new TxCompletePayload(channelId));
        var stream = new MemoryStream();
        var expectedBytes = "0x00460000000000000000000000000000000000000000000000000000000000000000".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}