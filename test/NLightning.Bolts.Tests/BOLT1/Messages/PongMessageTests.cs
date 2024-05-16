using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.Exceptions;
using Tests.Utils;

public class PongMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsPongMessageWithCorrectPayload()
    {
        // Arrange
        var expectedNumPongBytes = 21;
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00130015000000000000000000000000000000000000000000"));

        // Act
        var message = await PongMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedNumPongBytes, message.Payload.BytesLength);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => PongMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new PongMessage(21);
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00130015000000000000000000000000000000000000000000");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}