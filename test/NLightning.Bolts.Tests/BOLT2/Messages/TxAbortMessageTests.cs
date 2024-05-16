using System.Text;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.Types;
using Tests.Utils;

public class TxAbortMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAbortMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedData = Encoding.UTF8.GetBytes("Some error");
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x004A0000000000000000000000000000000000000000000000000000000000000000000A536F6D65206572726F72"));

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
        var data = Encoding.UTF8.GetBytes("Some error");
        var message = new TxAbortMessage(new TxAbortPayload(channelId, data));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x004A0000000000000000000000000000000000000000000000000000000000000000000A536F6D65206572726F72");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}