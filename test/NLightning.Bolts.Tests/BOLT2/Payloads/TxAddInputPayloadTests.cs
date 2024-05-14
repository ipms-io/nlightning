namespace NLightning.Bolts.Tests.BOLT2.Payloads;

using Bolts.BOLT2.Payloads;
using Common.Types;

public class TxAddInputPayloadTests
{
    [Fact]
    public void Given_SequenceOutOfBounds_When_Constructing_Then_ThrowsArgumentException()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        byte[] prevTx = [0x00, 0x01, 0x02, 0x03];
        uint prevTxVout = 0;
        var sequence = 0xFFFFFFFE;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence));
    }
}