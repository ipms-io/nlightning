using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;
using Tests.Utils;

public class PingMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsPingMessageWithCorrectPayload()
    {
        // Arrange
        var expectedBytesLength = 21;
        var expectedNumPongBytes = 42;
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x0012002A0015000000000000000000000000000000000000000000"));

        // Act
        var pingMessage = await PingMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(pingMessage);
        Assert.Equal(expectedBytesLength, pingMessage.Payload.BytesLength);
        Assert.Equal(expectedNumPongBytes, pingMessage.Payload.NumPongBytes);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => PingMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new PingMessage
        {
            Payload = new FakePingPayload(42, 21)
        };
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x0012002A0015000000000000000000000000000000000000000000");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}