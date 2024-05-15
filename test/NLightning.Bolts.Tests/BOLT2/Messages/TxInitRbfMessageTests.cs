namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Bolts.Exceptions;
using Common.TLVs;
using Common.Types;
using Tests.Utils;

public class TxInitRbfMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxInitRbfMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        uint expectedLocktime = 1;
        uint expectedFeerate = 1;
        var extension = new TLVStream();
        var expectedTlv = new FundingOutputContrubutionTLV(10);
        extension.Add(expectedTlv);
        var expectedTlv2 = new RequiredConfirmedInputsTLV();
        extension.Add(expectedTlv2);
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00480000000000000000000000000000000000000000000000000000000000000000000000010000000100000200"));

        // Act
        var message = await TxInitRbfMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedLocktime, message.Payload.Locktime);
        Assert.Equal(expectedFeerate, message.Payload.Feerate);
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
        await Assert.ThrowsAsync<MessageSerializationException>(() => TxInitRbfMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        uint locktime = 1;
        uint feerate = 1;
        var extension = new TLVStream();
        var tlv = new FundingOutputContrubutionTLV(10);
        extension.Add(tlv);
        var tlv2 = new RequiredConfirmedInputsTLV();
        extension.Add(tlv2);
        var message = new TxInitRbfMessage(new TxInitRbfPayload(channelId, locktime, feerate), extension);
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00480000000000000000000000000000000000000000000000000000000000000000000000010000000100000200");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}