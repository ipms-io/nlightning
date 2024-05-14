using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using BOLT1.Mock;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;
using Common.Types;
using Tests.Utils;

public class WarningMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsWarningMessageWithCorrectPayload()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var warningMessage = "Warning message!";
        var expectedData = Encoding.UTF8.GetBytes(warningMessage);
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x0001000000000000000000000000000000000000000000000000000000000000000000105761726E696E67206D65737361676521"));

        // Act
        var message = await WarningMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(warningMessage);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedData, message.Payload.Data);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => WarningMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_StreamReadWarning_When_DeserializeAsync_Then_ThrowsIOException()
    {
        // Arrange
        var brokenStream = new FakeBrokenStream();

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => WarningMessage.DeserializeAsync(brokenStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new WarningMessage(new ErrorPayload("Warning message!"));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x0001000000000000000000000000000000000000000000000000000000000000000000105761726E696E67206D65737361676521");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}