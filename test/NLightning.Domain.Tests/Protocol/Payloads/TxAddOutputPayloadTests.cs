using NBitcoin;

namespace NLightning.Domain.Tests.Protocol.Payloads;

using Domain.Protocol.Payloads;
using Domain.ValueObjects;

public class TxAddOutputPayloadTests
{
    [Fact]
    public void Given_SequenceOutOfBounds_When_Constructing_Then_ThrowsArgumentException()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        ulong serialId = 1;
        ulong sats = 0;
        var script = new Script();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TxAddOutputPayload(sats, channelId, script, serialId));
    }
}