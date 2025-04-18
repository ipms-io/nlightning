using NLightning.Bolts.Tests.Utils;
using NLightning.Common.Messages;
using NLightning.Common.Messages.Payloads;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

public class WarningMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsWarningMessageWithCorrectPayload()
    {
        // Arrange
        var expectedPayload = new ErrorPayload("Warning message!");
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000105761726E696E67206D65737361676521".ToByteArray());

        // Act
        var errorMessage = await WarningMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(errorMessage);
        Assert.Equal(expectedPayload.ChannelId, errorMessage.Payload.ChannelId);
        Assert.Equal(expectedPayload.Data, errorMessage.Payload.Data);
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new ErrorMessage(new ErrorPayload("Warning message!"));
        var stream = new MemoryStream();
        var expectedBytes = "0011000000000000000000000000000000000000000000000000000000000000000000105761726E696E67206D65737361676521".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}