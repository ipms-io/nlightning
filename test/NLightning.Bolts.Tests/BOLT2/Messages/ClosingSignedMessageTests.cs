using NBitcoin.Crypto;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Enums;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class ClosingSignedMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsClosingSignedMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedFeeSatoshis = LightningMoney.FromUnit(2, LightningMoneyUnit.SATOSHI);
        var expectedMinFee = LightningMoney.FromUnit(1, LightningMoneyUnit.SATOSHI);
        var expectedMaxFee = LightningMoney.FromUnit(3, LightningMoneyUnit.SATOSHI);
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".ToByteArray(), out var expectedSignature);
        var expectedSignatureBytes = expectedSignature.ToCompact().ToArray();

        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320011000000000000000010000000000000003".ToByteArray());

        // Act
        var message = await ClosingSignedMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedFeeSatoshis, message.Payload.FeeAmount);
        Assert.Equal(expectedSignatureBytes, message.Payload.Signature.ToCompact().ToArray());
        Assert.NotNull(message.Extension);
        Assert.Equal(expectedMinFee, message.FeeRangeTlv.MinFeeAmount);
        Assert.Equal(expectedMaxFee, message.FeeRangeTlv.MaxFeeAmount);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000024737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A73200110".ToByteArray());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ClosingSignedMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var feeSatoshis = LightningMoney.FromUnit(2, LightningMoneyUnit.SATOSHI);
        var minFee = LightningMoney.FromUnit(1, LightningMoneyUnit.SATOSHI);
        var maxFee = LightningMoney.FromUnit(3, LightningMoneyUnit.SATOSHI);
        _ = ECDSASignature.TryParseFromCompact("4737AF4C6314905296FD31D3610BD638F92C8A3687D0C6D845E3B9EF4957670733A30A9A81F924CD9F73F46805D0FB60D7C293FB2D8100DD3FA92B10934A7320".ToByteArray(), out var signature);
        var message = new ClosingSignedMessage(new ClosingSignedPayload(channelId, feeSatoshis, signature), new FeeRangeTlv(minFee, maxFee));
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
    #endregion
}