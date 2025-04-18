using NBitcoin;
using NLightning.Common.Messages;
using NLightning.Common.Messages.Payloads;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Common.Types;
using Utils;

public class RevokeAndAckMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsRevokeAndAckMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var perCommitmentSecret = "c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray();
        var nextPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());

        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A7502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray());

        // Act
        var message = await RevokeAndAckMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(perCommitmentSecret, message.Payload.PerCommitmentSecret);
        Assert.Equal(nextPerCommitmentPoint, message.Payload.NextPerCommitmentPoint);
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var perCommitmentSecret = "c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray();
        var nextPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var message = new RevokeAndAckMessage(new RevokeAndAckPayload(channelId, perCommitmentSecret, nextPerCommitmentPoint));
        var stream = new MemoryStream();
        var expectedBytes = "00850000000000000000000000000000000000000000000000000000000000000000C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A7502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}