namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class StfuMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsStfuMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedInitiator = true;

        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000001".ToByteArray());

        // Act
        var message = await StfuMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedInitiator, message.Payload.Initiator);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => StfuMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var message = new StfuMessage(new StfuPayload(channelId, true));
        var stream = new MemoryStream();
        var expectedBytes = "0002000000000000000000000000000000000000000000000000000000000000000001".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}