namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class TxAddOutputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAddOutputMessage()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong SERIAL_ID = 1;
        const ulong SATS = 1000;
        byte[] script = [0x00, 0x01, 0x02, 0x03];

        var stream = new MemoryStream("0x0000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000003E8000400010203".ToByteArray());

        // Act
        var message = await TxAddOutputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(channelId, message.Payload.ChannelId);
        Assert.Equal(SERIAL_ID, message.Payload.SerialId);
        Assert.Equal(SATS, message.Payload.Sats);
        Assert.Equal(script, message.Payload.Script);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxAddOutputMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong SERIAL_ID = 1;
        const ulong SATS = 1000;
        byte[] script = [0x00, 0x01, 0x02, 0x03];
        var message = new TxAddOutputMessage(new TxAddOutputPayload(channelId, SERIAL_ID, SATS, script));
        var stream = new MemoryStream();
        var expectedBytes = "0x00430000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000003E8000400010203".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}