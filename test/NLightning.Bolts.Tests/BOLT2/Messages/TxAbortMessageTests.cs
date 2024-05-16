namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class TxAbortMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAbortMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedData = "Some error"u8.ToArray();
        var stream = new MemoryStream("0x0000000000000000000000000000000000000000000000000000000000000000000A536F6D65206572726F72".ToByteArray());

        // Act
        var message = await TxAbortMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedData, message.Payload.Data);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxAbortMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var data = "Some error"u8.ToArray();
        var message = new TxAbortMessage(new TxAbortPayload(channelId, data));
        var stream = new MemoryStream();
        var expectedBytes = "0x004A0000000000000000000000000000000000000000000000000000000000000000000A536F6D65206572726F72".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}