namespace NLightning.Domain.Tests.Protocol.Payloads;

using Domain.Channels.ValueObjects;
using Domain.Protocol.Payloads;

public class TxAddInputPayloadTests
{
    [Fact]
    public void Given_SequenceOutOfBounds_When_Constructing_Then_ThrowsArgumentException()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong serialId = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        const uint prevTxVout = 0;
        const uint sequence = 0xFFFFFFFE;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout,
                                                                     sequence));
    }
}