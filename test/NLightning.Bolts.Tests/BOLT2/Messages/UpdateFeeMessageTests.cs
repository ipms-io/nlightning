namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Utils;

public class UpdateFeeMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateFeeMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedFeeratePerKw = 10U;

        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000A".ToByteArray());

        // Act
        var message = await UpdateFeeMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedFeeratePerKw, message.Payload.FeeratePerKw);
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var feeratePerKw = 10U;
        var message = new UpdateFeeMessage(new UpdateFeePayload(channelId, feeratePerKw));
        var stream = new MemoryStream();
        var expectedBytes = "008600000000000000000000000000000000000000000000000000000000000000000000000A".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}