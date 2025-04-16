using NLightning.Common.Messages.Payloads;

namespace NLightning.Bolts.Tests.BOLT2.Payloads;

using Common.Types;

public class TxAddOutputPayloadTests
{
    [Fact]
    public void Given_SequenceOutOfBounds_When_Constructing_Then_ThrowsArgumentException()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        ulong sats = 0;
        var script = new byte[ushort.MaxValue + 1];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TxAddOutputPayload(channelId, serialId, sats, script));
    }
}