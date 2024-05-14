using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Exceptions;
using Mock;
using Common.Types;
using Utils;

public class ErrorMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsErrorMessageWithCorrectPayload()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var errorMessage = "Error message!";
        var expectedData = Encoding.UTF8.GetBytes(errorMessage);
        var stream = new MemoryStream("0x0000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521".ToByteArray());

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
        var invalidStream = new MemoryStream("Invalid content"u8.ToArray());

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
        var expectedBytes = "0x00110000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}