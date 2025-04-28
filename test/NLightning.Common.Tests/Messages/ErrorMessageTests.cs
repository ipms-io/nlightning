using System.Text;

namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
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
        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521".GetBytes());

        // Act
        var message = await ErrorMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedData, message.Payload.Data);
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new ErrorMessage(new ErrorPayload("Error message!"));
        var stream = new MemoryStream();
        var expectedBytes = "00110000000000000000000000000000000000000000000000000000000000000000000E4572726F72206D65737361676521".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}