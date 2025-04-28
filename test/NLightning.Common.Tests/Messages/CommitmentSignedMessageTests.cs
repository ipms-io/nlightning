using NBitcoin.Crypto;

namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Types;
using Utils;

public class CommitmentSignedMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsCommitmentSignedMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const int NUM = 2;
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".GetBytes(), out var expectedSignature);
        var expectedSignatureBytes = expectedSignature.ToCompact().ToArray();

        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000004737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A732000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A73204737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".GetBytes());

        // Act
        var message = await CommitmentSignedMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedSignatureBytes, message.Payload.Signature.ToCompact().ToArray());
        Assert.Equal(NUM, message.Payload.NumHtlcs);
        Assert.NotEmpty(message.Payload.HtlcSignatures);
        foreach (var htlcSignature in message.Payload.HtlcSignatures)
        {
            Assert.Equal(expectedSignatureBytes, htlcSignature.ToCompact().ToArray());
        }
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".GetBytes(), out var signature);
        var message = new CommitmentSignedMessage(new CommitmentSignedPayload(channelId, signature, new List<ECDSASignature> { signature, signature }));
        var stream = new MemoryStream();
        var expectedBytes = "008400000000000000000000000000000000000000000000000000000000000000004737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A732000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A73204737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}