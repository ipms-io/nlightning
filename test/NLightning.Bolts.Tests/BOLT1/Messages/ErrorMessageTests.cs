using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using BOLT1.Mock;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;
using Common.Types;
using Tests.Utils;

public class ErrorMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsErrorMessageWithCorrectPayload()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var errorMessage = "Error message!";
        var expectedData = Encoding.UTF8.GetBytes(errorMessage);
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00110000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521"));

        // Act
        var message = await ErrorMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedData, message.Payload.Data);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ErrorMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_StreamReadError_When_DeserializeAsync_Then_ThrowsIOException()
    {
        // Arrange
        var brokenStream = new FakeBrokenStream(); // You would need to mock or implement a stream that simulates a read error.

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ErrorMessage.DeserializeAsync(brokenStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new ErrorMessage(new ErrorPayload("Error message!"));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00110000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}