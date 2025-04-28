namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Utils;

public class TxRemoveOutputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxRemoveOutputMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const ulong EXPECTED_SERIAL_ID = 1;
        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000000000001".GetBytes());

        // Act
        var message = await TxRemoveOutputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_SERIAL_ID, message.Payload.SerialId);
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        var message = new TxRemoveOutputMessage(new TxRemoveOutputPayload(channelId, serialId));
        var stream = new MemoryStream();
        var expectedBytes = "004500000000000000000000000000000000000000000000000000000000000000000000000000000001".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}