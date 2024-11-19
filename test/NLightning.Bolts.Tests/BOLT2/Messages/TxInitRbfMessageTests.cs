namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class TxInitRbfMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxInitRbfMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const uint EXPECTED_LOCKTIME = 1;
        const uint EXPECTED_FEERATE = 1;
        var expectedTlv = new FundingOutputContributionTlv(10);
        var expectedTlv2 = new RequireConfirmedInputsTlv();
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000001000000010008000000000000000A0200".ToByteArray());

        // Act
        var message = await TxInitRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_LOCKTIME, message.Payload.Locktime);
        Assert.Equal(EXPECTED_FEERATE, message.Payload.Feerate);
        Assert.NotNull(message.FundingOutputContribution);
        Assert.Equal(expectedTlv, message.FundingOutputContribution);
        Assert.NotNull(message.RequireConfirmedInputs);
        Assert.Equal(expectedTlv2, message.RequireConfirmedInputs);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxInitRbfMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const uint LOCKTIME = 1;
        const uint FEERATE = 1;
        var message = new TxInitRbfMessage(new TxInitRbfPayload(channelId, LOCKTIME, FEERATE), new FundingOutputContributionTlv(10), new RequireConfirmedInputsTlv());
        var stream = new MemoryStream();
        var expectedBytes = "0048000000000000000000000000000000000000000000000000000000000000000000000001000000010008000000000000000A0200".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}