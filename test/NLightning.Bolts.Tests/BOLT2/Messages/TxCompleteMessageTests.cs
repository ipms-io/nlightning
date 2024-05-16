namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.Types;
using Tests.Utils;

public class TxCompleteMessageTests
{
    [Fact]
    public async Task DeserializeAsync_GivenValidStream_ReturnsTxCompleteMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00460000000000000000000000000000000000000000000000000000000000000000"));

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
        var expectedBytes = TestHexConverter.ToByteArray("0x00460000000000000000000000000000000000000000000000000000000000000000");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}