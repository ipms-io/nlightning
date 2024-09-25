namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class TxRemoveOutputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxRemoveOutputMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const ulong EXPECTED_SERIAL_ID = 1;
        var stream = new MemoryStream("0x00000000000000000000000000000000000000000000000000000000000000000000000000000001".ToByteArray());

        // Act
        var message = await TxRemoveOutputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_SERIAL_ID, message.Payload.SerialId);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxRemoveOutputMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        var message = new TxRemoveOutputMessage(new TxRemoveOutputPayload(channelId, serialId));
        var stream = new MemoryStream();
        var expectedBytes = "0x004500000000000000000000000000000000000000000000000000000000000000000000000000000001".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}