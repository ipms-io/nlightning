namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Utils;

public class UpdateFulfillHtlcMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateFulfillHtlcMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedPaymentPreimage = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".GetBytes();
        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000000000000567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA5".GetBytes());

        // Act
        var message = await UpdateFulfillHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedPaymentPreimage, message.Payload.PaymentPreimage);
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
        var paymentPreimage = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".GetBytes();
        var message = new UpdateFulfillHtlcMessage(new UpdateFulfillHtlcPayload(channelId, id, paymentPreimage));
        var stream = new MemoryStream();
        var expectedBytes = "008200000000000000000000000000000000000000000000000000000000000000000000000000000000567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA5".GetBytes();

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