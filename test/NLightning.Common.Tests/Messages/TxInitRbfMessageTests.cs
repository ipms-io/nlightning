namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Exceptions;
using Utils;

public class TxInitRbfMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxInitRbfMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const uint EXPECTED_LOCKTIME = 1;
        const uint EXPECTED_FEERATE = 1;
        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000100000001".GetBytes());

        // Act
        var message = await TxInitRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_LOCKTIME, message.Payload.Locktime);
        Assert.Equal(EXPECTED_FEERATE, message.Payload.Feerate);
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxInitRbfMessageWithExtensions()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const uint EXPECTED_LOCKTIME = 1;
        const uint EXPECTED_FEERATE = 1;
        var expectedTlv = new FundingOutputContributionTlv(10);
        var expectedTlv2 = new RequireConfirmedInputsTlv();
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000001000000010008000000000000000A0200".GetBytes());

        // Act
        var message = await TxInitRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_LOCKTIME, message.Payload.Locktime);
        Assert.Equal(EXPECTED_FEERATE, message.Payload.Feerate);
        Assert.NotNull(message.FundingOutputContributionTlv);
        Assert.Equal(expectedTlv, message.FundingOutputContributionTlv);
        Assert.NotNull(message.RequireConfirmedInputsTlv);
        Assert.Equal(expectedTlv2, message.RequireConfirmedInputsTlv);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000001000000010002".GetBytes());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxInitRbfMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const uint LOCKTIME = 1;
        const uint FEERATE = 1;
        var message = new TxInitRbfMessage(new TxInitRbfPayload(channelId, LOCKTIME, FEERATE));
        var stream = new MemoryStream();
        var expectedBytes = "004800000000000000000000000000000000000000000000000000000000000000000000000100000001".GetBytes();

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
        const uint LOCKTIME = 1;
        const uint FEERATE = 1;
        var fundingOutputContributionTlv = new FundingOutputContributionTlv(10);
        var requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();
        var message = new TxInitRbfMessage(new TxInitRbfPayload(channelId, LOCKTIME, FEERATE), fundingOutputContributionTlv, requireConfirmedInputsTlv);
        var stream = new MemoryStream();
        var expectedBytes = "0048000000000000000000000000000000000000000000000000000000000000000000000001000000010008000000000000000A0200".GetBytes();

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