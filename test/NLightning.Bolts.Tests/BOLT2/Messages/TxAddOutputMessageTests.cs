namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.Types;
using NLightning.Bolts.Tests.Utils;

public class TxAddOutputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAddOutputMessage()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        ulong sats = 1000;
        byte[] script = [0x00, 0x01, 0x02, 0x03];

        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00430000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000003E8000400010203"));

        // Act
        var message = await TxAddOutputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(channelId, message.Payload.ChannelId);
        Assert.Equal(serialId, message.Payload.SerialId);
        Assert.Equal(sats, message.Payload.Sats);
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
        ulong serialId = 1;
        ulong sats = 1000;
        byte[] script = [0x00, 0x01, 0x02, 0x03];
        var message = new TxAddOutputMessage(new TxAddOutputPayload(channelId, serialId, sats, script));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00430000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000003E8000400010203");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}