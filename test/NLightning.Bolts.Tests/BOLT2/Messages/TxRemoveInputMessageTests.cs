namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.Types;
using Tests.Utils;

public class TxRemoveInputMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxRemoveInputMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        ulong expectedSerialId = 1;
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x004400000000000000000000000000000000000000000000000000000000000000000000000000000001"));

        // Act
        var message = await TxRemoveInputMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedSerialId, message.Payload.SerialId);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxRemoveInputMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        var message = new TxRemoveInputMessage(new TxRemoveInputPayload(channelId, serialId));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x004400000000000000000000000000000000000000000000000000000000000000000000000000000001");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}