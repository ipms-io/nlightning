using NBitcoin.Crypto;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class ClosingSignedMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsClosingSignedMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const ulong EXPECTED_FEE_SATOSHIS = 2UL;
        const ulong EXPECTED_MIN_FEE = 1UL;
        const ulong EXPECTED_MAX_FEE = 3UL;
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".ToByteArray(), out var expectedSignature);
        var expectedSignatureBytes = expectedSignature.ToCompact().ToArray();

        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320011000000000000000010000000000000003".ToByteArray());

        // Act
        var message = await ClosingSignedMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(EXPECTED_FEE_SATOSHIS, message.Payload.FeeSatoshis);
        Assert.Equal(expectedSignatureBytes, message.Payload.Signature.ToCompact().ToArray());
        Assert.Equal(EXPECTED_MIN_FEE, message.FeeRange.MinFeeSatoshis);
        Assert.Equal(EXPECTED_MAX_FEE, message.FeeRange.MaxFeeSatoshis);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ClosingSignedMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong FEE_SATOSHIS = 2UL;
        const ulong MIN_FEE = 1UL;
        const ulong MAX_FEE = 3UL;
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".ToByteArray(), out var signature);
        var message = new ClosingSignedMessage(new ClosingSignedPayload(channelId, FEE_SATOSHIS, signature), new FeeRangeTlv(MIN_FEE, MAX_FEE));
        var stream = new MemoryStream();
        var expectedBytes = "0027000000000000000000000000000000000000000000000000000000000000000000000000000000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320011000000000000000010000000000000003".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}