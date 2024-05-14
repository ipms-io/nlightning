namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.Types;
using NLightning.Bolts.Tests.Utils;

public class TxAddInputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAddInputMessage()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        uint prevTxVout = 0;
        var sequence = 0xFFFFFFFD;

        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x0000000000000000000000000000000000000000000000000000000000000000000000000000000100040001020300000000FFFFFFFD"));

        // Act
        var message = await TxAddInputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(channelId, message.Payload.ChannelId);
        Assert.Equal(serialId, message.Payload.SerialId);
        Assert.Equal(prevTx, message.Payload.PrevTx);
        Assert.Equal(prevTxVout, message.Payload.PrevTxVout);
        Assert.Equal(sequence, message.Payload.Sequence);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxAddInputMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        uint prevTxVout = 0;
        var sequence = 0xFFFFFFFD;
        var message = new TxAddInputMessage(new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00420000000000000000000000000000000000000000000000000000000000000000000000000000000100040001020300000000FFFFFFFD");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}