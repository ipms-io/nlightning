namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class TxAckRbfMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxAckRbfMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var extension = new TlvStream();
        var expectedTlv = new FundingOutputContributionTlv(10);
        extension.Add(expectedTlv);
        var expectedTlv2 = new RequiredConfirmedInputsTlv();
        extension.Add(expectedTlv2);
        var stream = new MemoryStream("0x000000000000000000000000000000000000000000000000000000000000000000000200".ToByteArray());

        // Act
        var message = await TxAckRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.NotNull(message.Extension);
        var tlvs = message.Extension.GetTlvs().ToList();
        Assert.Equal(2, tlvs.Count);
        Assert.Equal(expectedTlv, tlvs[0]);
        Assert.Equal(expectedTlv2, tlvs[1]);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxAckRbfMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var extension = new TlvStream();
        var tlv = new FundingOutputContributionTlv(10);
        extension.Add(tlv);
        var tlv2 = new RequiredConfirmedInputsTlv();
        extension.Add(tlv2);
        var message = new TxAckRbfMessage(new TxAckRbfPayload(channelId), extension);
        var stream = new MemoryStream();
        var expectedBytes = "0x0049000000000000000000000000000000000000000000000000000000000000000000000200".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}