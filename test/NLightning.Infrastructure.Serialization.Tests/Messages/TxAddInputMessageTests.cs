using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Serialization.Tests.Messages;

using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Helpers;
using Serialization.Messages.Types;

public class TxAddInputMessageTests
{
    private readonly TxAddInputMessageTypeSerializer _txAddInputMessageTypeSerializer;

    public TxAddInputMessageTests()
    {
        _txAddInputMessageTypeSerializer =
            new TxAddInputMessageTypeSerializer(SerializerHelper.PayloadSerializerFactory);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAddInputMessage()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong SERIAL_ID = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        const uint PREV_TX_VOUT = 0;
        const uint SEQUENCE = 0xFFFFFFFD;

        var stream = new MemoryStream(Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000000000000000000100040001020300000000FFFFFFFD"));

        // Act
        var message = await _txAddInputMessageTypeSerializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(channelId, message.Payload.ChannelId);
        Assert.Equal(SERIAL_ID, message.Payload.SerialId);
        Assert.Equal(prevTx, message.Payload.PrevTx);
        Assert.Equal(PREV_TX_VOUT, message.Payload.PrevTxVout);
        Assert.Equal(SEQUENCE, message.Payload.Sequence);
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong SERIAL_ID = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        const uint PREV_TX_VOUT = 0;
        const uint SEQUENCE = 0xFFFFFFFD;
        var message = new TxAddInputMessage(new TxAddInputPayload(channelId, SERIAL_ID, prevTx, PREV_TX_VOUT, SEQUENCE));
        var stream = new MemoryStream();
        var expectedBytes = Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000000000000000000100040001020300000000FFFFFFFD");

        // Act
        await _txAddInputMessageTypeSerializer.SerializeAsync(message, stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}