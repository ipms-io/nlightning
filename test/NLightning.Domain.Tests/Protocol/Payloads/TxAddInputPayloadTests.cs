namespace NLightning.Domain.Tests.Protocol.Payloads;

using Domain.Protocol.Payloads;
using Domain.ValueObjects;

public class TxAddInputPayloadTests
{
    [Fact]
    public void Given_SequenceOutOfBounds_When_Constructing_Then_ThrowsArgumentException()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong SERIAL_ID = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        const uint PREV_TX_VOUT = 0;
        const uint SEQUENCE = 0xFFFFFFFE;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TxAddInputPayload(channelId, SERIAL_ID, prevTx, PREV_TX_VOUT,
                                                                     SEQUENCE));
    }
}