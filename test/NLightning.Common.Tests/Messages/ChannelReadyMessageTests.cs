using NBitcoin;

namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class ChannelReadyMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsChannelReadyMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000003A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD".GetBytes());

        // Act
        var message = await ChannelReadyMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsChannelReadyMessageWithExtension()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedTlv = new ShortChannelIdTlv(new ShortChannelId(1234, 0, 1));
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000003A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD01080004D20000000001".GetBytes());

        // Act
        var message = await ChannelReadyMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.NotNull(message.Extension);
        Assert.NotNull(message.ShortChannelIdTlv);
        Assert.Equal(expectedTlv, message.ShortChannelIdTlv);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000003A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD010800".GetBytes());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ChannelReadyMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var secondPerCommitmentPoint = new PubKey("03a92b07cbae641dcfd482825233aecc2d5012913b48040131db3222670c2bffcd".GetBytes());
        var message = new ChannelReadyMessage(new ChannelReadyPayload(channelId, secondPerCommitmentPoint));
        var stream = new MemoryStream();
        var expectedBytes = "0049000000000000000000000000000000000000000000000000000000000000000003A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidExtension_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var secondPerCommitmentPoint = new PubKey("03a92b07cbae641dcfd482825233aecc2d5012913b48040131db3222670c2bffcd".GetBytes());
        var shortChannelIdTlv = new ShortChannelIdTlv(new ShortChannelId(1234, 0, 1));
        var message = new ChannelReadyMessage(new ChannelReadyPayload(channelId, secondPerCommitmentPoint), shortChannelIdTlv);
        var stream = new MemoryStream();
        var expectedBytes = "0049000000000000000000000000000000000000000000000000000000000000000003A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD01080004D20000000001".GetBytes();

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