namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Utils;

public class UpdateFailHtlcMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateFailHtlcMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedReason = "567cbdadb00b825448b2e414487d73".GetBytes();
        var expectedLen = expectedReason.Length;
        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000000000000000F567CBDADB00B825448B2E414487D73".GetBytes());

        // Act
        var message = await UpdateFailHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedLen, message.Payload.Len);
        Assert.Equal(expectedReason, message.Payload.Reason);
        Assert.Null(message.Extension);
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayloadWith_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var id = 0UL;
        var expectedReason = "567cbdadb00b825448b2e414487d73".GetBytes();
        var message = new UpdateFailHtlcMessage(new UpdateFailHtlcPayload(channelId, id, expectedReason));
        var stream = new MemoryStream();
        var expectedBytes = "008300000000000000000000000000000000000000000000000000000000000000000000000000000000000F567CBDADB00B825448B2E414487D73".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
    #endregion
}