using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Utils;

public class ShutdownMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsShutdownMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedLength = 22;
        var expectedScriptPubkeyBytes = "00141B25836987ECA16276373ACEEF68AD9ED538EA9D".ToByteArray();

        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000001600141B25836987ECA16276373ACEEF68AD9ED538EA9D".ToByteArray());

        // Act
        var message = await ShutdownMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedLength, message.Payload.ScriptPubkeyLen);
        Assert.Equal(expectedScriptPubkeyBytes, message.Payload.ScriptPubkey.ToBytes());
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var scriptPubkey = new Key("E0A724A27146D791D59117F3D6E07B7C1F9E161BE8AF7B06622221DEB5798FD4".ToByteArray()).GetScriptPubKey(ScriptPubKeyType.Segwit);
        var message = new ShutdownMessage(new ShutdownPayload(channelId, scriptPubkey));
        var stream = new MemoryStream();
        var expectedBytes = "00260000000000000000000000000000000000000000000000000000000000000000001600141B25836987ECA16276373ACEEF68AD9ED538EA9D".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}