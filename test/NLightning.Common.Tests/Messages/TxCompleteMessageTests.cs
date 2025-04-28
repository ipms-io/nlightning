namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Utils;

public class TxCompleteMessageTests
{
    [Fact]
    public async Task DeserializeAsync_GivenValidStream_ReturnsTxCompleteMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000".GetBytes());

        // Act
        var message = await TxCompleteMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
    }

    [Fact]
    public async Task SerializeAsync_GivenValidPayload_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var message = new TxCompleteMessage(new TxCompletePayload(channelId));
        var stream = new MemoryStream();
        var expectedBytes = "00460000000000000000000000000000000000000000000000000000000000000000".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}