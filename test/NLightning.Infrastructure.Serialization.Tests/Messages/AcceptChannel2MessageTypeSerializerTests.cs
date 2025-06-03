using NBitcoin;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Serialization.Tests.Messages;

using Domain.Money;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Protocol.Tlv;
using Domain.ValueObjects;
using Exceptions;
using Helpers;
using Serialization.Messages.Types;

public class AcceptChannel2MessageTypeSerializerTests
{
    private readonly LightningMoney _expectedDustLimitAmount = LightningMoney.Satoshis(1);
    private const ushort EXPECTED_TO_SELF_DELAY = 1;
    private readonly LightningMoney _expectedHtlcMinimumAmount = LightningMoney.Satoshis(1);
    private const ushort EXPECTED_MINIMUM_DEPTH = 3;
    private readonly LightningMoney _expectedMaxHtlcValueInFlightAmount = LightningMoney.Satoshis(1_000);
    private const ushort EXPECTED_MAX_ACCEPTED_HTLCS = 2;
    private readonly AcceptChannel2MessageTypeSerializer _acceptChannel2TypeSerializer;

    public AcceptChannel2MessageTypeSerializerTests()
    {
        _acceptChannel2TypeSerializer =
            new AcceptChannel2MessageTypeSerializer(SerializerHelper.PayloadSerializerFactory,
                                                    SerializerHelper.TlvConverterFactory,
                                                    SerializerHelper.TlvStreamSerializer);
    }

    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsAcceptChannel2Message()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedFundingSatoshis = LightningMoney.Zero;
        var expectedFundingPubKey = new PubKey(Convert.FromHexString("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75"));
        var expectedRevocationBasepoint = new PubKey(Convert.FromHexString("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534"));
        var expectedPaymentBasePoint = new PubKey(Convert.FromHexString("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1"));
        var expectedDelayedPaymentBasepoint = new PubKey(Convert.FromHexString("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce"));
        var expectedHtlcBasepoint = new PubKey(Convert.FromHexString("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b"));
        var expectedFirstPerCommitmentPoint = new PubKey(Convert.FromHexString("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61"));

        var stream = new MemoryStream(Convert.FromHexString("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F61"));

        // Act
        var result = await _acceptChannel2TypeSerializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChannelId, result.Payload.ChannelId);
        Assert.Equal(expectedFundingSatoshis, result.Payload.FundingAmount);
        Assert.Equal(_expectedDustLimitAmount, result.Payload.DustLimitAmount);
        Assert.Equal(_expectedMaxHtlcValueInFlightAmount, result.Payload.MaxHtlcValueInFlightAmount);
        Assert.Equal(_expectedHtlcMinimumAmount, result.Payload.HtlcMinimumAmount);
        Assert.Equal(EXPECTED_MINIMUM_DEPTH, result.Payload.MinimumDepth);
        Assert.Equal(EXPECTED_TO_SELF_DELAY, result.Payload.ToSelfDelay);
        Assert.Equal(EXPECTED_MAX_ACCEPTED_HTLCS, result.Payload.MaxAcceptedHtlcs);
        Assert.Equal(expectedFundingPubKey, result.Payload.FundingPubKey);
        Assert.Equal(expectedRevocationBasepoint, result.Payload.RevocationBasepoint);
        Assert.Equal(expectedPaymentBasePoint, result.Payload.PaymentBasepoint);
        Assert.Equal(expectedDelayedPaymentBasepoint, result.Payload.DelayedPaymentBasepoint);
        Assert.Equal(expectedHtlcBasepoint, result.Payload.HtlcBasepoint);
        Assert.Equal(expectedFirstPerCommitmentPoint, result.Payload.FirstPerCommitmentPoint);
        Assert.Null(result.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsAcceptChannel2MessageWithExtensions()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedFundingSatoshis = LightningMoney.Zero;
        var expectedFundingPubKey = new PubKey(Convert.FromHexString("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75"));
        var expectedRevocationBasepoint = new PubKey(Convert.FromHexString("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534"));
        var expectedPaymentBasePoint = new PubKey(Convert.FromHexString("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1"));
        var expectedDelayedPaymentBasepoint = new PubKey(Convert.FromHexString("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce"));
        var expectedHtlcBasepoint = new PubKey(Convert.FromHexString("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b"));
        var expectedFirstPerCommitmentPoint = new PubKey(Convert.FromHexString("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61"));
        var expectedUpfrontShutdownScriptTlv = new UpfrontShutdownScriptTlv(expectedFundingPubKey.ScriptPubKey);
        var expectedChannelTypeTlv = new ChannelTypeTlv([0x01, 0x02]);

        var stream = new MemoryStream(Convert.FromHexString("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F6100232102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75AC010201020200"));

        // Act
        var result = await _acceptChannel2TypeSerializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChannelId, result.Payload.ChannelId);
        Assert.Equal(expectedFundingSatoshis, result.Payload.FundingAmount);
        Assert.Equal(_expectedDustLimitAmount, result.Payload.DustLimitAmount);
        Assert.Equal(_expectedMaxHtlcValueInFlightAmount, result.Payload.MaxHtlcValueInFlightAmount);
        Assert.Equal(_expectedHtlcMinimumAmount, result.Payload.HtlcMinimumAmount);
        Assert.Equal(EXPECTED_MINIMUM_DEPTH, result.Payload.MinimumDepth);
        Assert.Equal(EXPECTED_TO_SELF_DELAY, result.Payload.ToSelfDelay);
        Assert.Equal(EXPECTED_MAX_ACCEPTED_HTLCS, result.Payload.MaxAcceptedHtlcs);
        Assert.Equal(expectedFundingPubKey, result.Payload.FundingPubKey);
        Assert.Equal(expectedRevocationBasepoint, result.Payload.RevocationBasepoint);
        Assert.Equal(expectedPaymentBasePoint, result.Payload.PaymentBasepoint);
        Assert.Equal(expectedDelayedPaymentBasepoint, result.Payload.DelayedPaymentBasepoint);
        Assert.Equal(expectedHtlcBasepoint, result.Payload.HtlcBasepoint);
        Assert.Equal(expectedFirstPerCommitmentPoint, result.Payload.FirstPerCommitmentPoint);
        Assert.NotNull(result.Extension);
        Assert.NotNull(result.UpfrontShutdownScriptTlv);
        Assert.Equal(expectedUpfrontShutdownScriptTlv, result.UpfrontShutdownScriptTlv);
        Assert.NotNull(result.ChannelTypeTlv);
        Assert.Equal(expectedChannelTypeTlv, result.ChannelTypeTlv);
        Assert.NotNull(result.RequireConfirmedInputsTlv);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_Then_ThrowsSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Convert.FromHexString("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F610023"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => _acceptChannel2TypeSerializer.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var fundingSatoshis = LightningMoney.Zero;
        var fundingPubKey = new PubKey(Convert.FromHexString("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75"));
        var revocationBasepoint = new PubKey(Convert.FromHexString("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534"));
        var paymentBasePoint = new PubKey(Convert.FromHexString("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1"));
        var delayedPaymentBasepoint = new PubKey(Convert.FromHexString("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce"));
        var htlcBasepoint = new PubKey(Convert.FromHexString("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b"));
        var firstPerCommitmentPoint = new PubKey(Convert.FromHexString("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61"));

        var message = new AcceptChannel2Message(
            new AcceptChannel2Payload(delayedPaymentBasepoint, _expectedDustLimitAmount, firstPerCommitmentPoint,
                                      fundingSatoshis, fundingPubKey, htlcBasepoint, _expectedHtlcMinimumAmount,
                                      EXPECTED_MAX_ACCEPTED_HTLCS, _expectedMaxHtlcValueInFlightAmount,
                                      EXPECTED_MINIMUM_DEPTH, paymentBasePoint, revocationBasepoint,
                                      channelId, EXPECTED_TO_SELF_DELAY));
        var stream = new MemoryStream();
        var expectedBytes = Convert.FromHexString("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F61");

        // Act
        await _acceptChannel2TypeSerializer.SerializeAsync(message, stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidExtension_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var fundingSatoshis = LightningMoney.Zero;
        var fundingPubKey = new PubKey(Convert.FromHexString("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75"));
        var revocationBasepoint = new PubKey(Convert.FromHexString("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534"));
        var paymentBasePoint = new PubKey(Convert.FromHexString("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1"));
        var delayedPaymentBasepoint = new PubKey(Convert.FromHexString("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce"));
        var htlcBasepoint = new PubKey(Convert.FromHexString("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b"));
        var firstPerCommitmentPoint = new PubKey(Convert.FromHexString("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61"));
        var upfrontShutdownScriptTlv = new UpfrontShutdownScriptTlv(fundingPubKey.ScriptPubKey);
        var channelTypeTlv = new ChannelTypeTlv([0x01, 0x02]);
        var requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();

        var message = new AcceptChannel2Message(
            new AcceptChannel2Payload(delayedPaymentBasepoint, _expectedDustLimitAmount, firstPerCommitmentPoint,
                                      fundingSatoshis, fundingPubKey, htlcBasepoint, _expectedHtlcMinimumAmount,
                                      EXPECTED_MAX_ACCEPTED_HTLCS, _expectedMaxHtlcValueInFlightAmount,
                                      EXPECTED_MINIMUM_DEPTH, paymentBasePoint, revocationBasepoint,
                                      channelId, EXPECTED_TO_SELF_DELAY),
            upfrontShutdownScriptTlv, channelTypeTlv, requireConfirmedInputsTlv);
        var stream = new MemoryStream();
        var expectedBytes = Convert.FromHexString("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F6100232102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75AC010201020200");

        // Act
        await _acceptChannel2TypeSerializer.SerializeAsync(message, stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
    #endregion
}