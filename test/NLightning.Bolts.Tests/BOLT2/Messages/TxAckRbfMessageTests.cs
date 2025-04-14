namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Exceptions;
using Common.TLVs;
using Common.Types;
using Utils;

public class TxAckRbfMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAckRbfMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000".ToByteArray());

        // Act
        var message = await TxAckRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAckRbfMessageWithExtensions()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedTlv = new FundingOutputContributionTlv(10);
        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000008000000000000000A0200".ToByteArray());

        // Act
        var message = await TxAckRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.NotNull(message.Extension);
        Assert.NotNull(message.FundingOutputContributionTlv);
        Assert.Equal(expectedTlv, message.FundingOutputContributionTlv);
        Assert.NotNull(message.RequireConfirmedInputsTlv);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000002".ToByteArray());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxAckRbfMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var message = new TxAckRbfMessage(new TxAckRbfPayload(channelId));
        var stream = new MemoryStream();
        var expectedBytes = "00490000000000000000000000000000000000000000000000000000000000000000".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidExtensions_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var fundingOutputContributionTlv = new FundingOutputContributionTlv(10);
        var requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();
        var message = new TxAckRbfMessage(new TxAckRbfPayload(channelId), fundingOutputContributionTlv, requireConfirmedInputsTlv);
        var stream = new MemoryStream();
        var expectedBytes = "004900000000000000000000000000000000000000000000000000000000000000000008000000000000000A0200".ToByteArray();

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