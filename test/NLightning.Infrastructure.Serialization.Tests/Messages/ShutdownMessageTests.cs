using NBitcoin;

namespace NLightning.Infrastructure.Serialization.Tests.Messages;

using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.ValueObjects;
using Helpers;
using Serialization.Messages.Types;

public class ShutdownMessageTests
{
    private readonly ShutdownMessageTypeSerializer _shutdownMessageTypeSerializer;
    
    public ShutdownMessageTests()
    {
        _shutdownMessageTypeSerializer =
            new ShutdownMessageTypeSerializer(SerializerHelper.PayloadSerializerFactory);
    }
    
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsShutdownMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedLength = 22;
        var expectedScriptPubkeyBytes = Convert.FromHexString("00141B25836987ECA16276373ACEEF68AD9ED538EA9D");

        var stream = new MemoryStream(Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000001600141B25836987ECA16276373ACEEF68AD9ED538EA9D"));

        // Act
        var message = await _shutdownMessageTypeSerializer.DeserializeAsync(stream);

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
        var scriptPubkey = new Key(Convert.FromHexString("E0A724A27146D791D59117F3D6E07B7C1F9E161BE8AF7B06622221DEB5798FD4")).GetScriptPubKey(ScriptPubKeyType.Segwit);
        var message = new ShutdownMessage(new ShutdownPayload(channelId, scriptPubkey));
        var stream = new MemoryStream();
        var expectedBytes = Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000001600141B25836987ECA16276373ACEEF68AD9ED538EA9D");

        // Act
        await _shutdownMessageTypeSerializer.SerializeAsync(message, stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}