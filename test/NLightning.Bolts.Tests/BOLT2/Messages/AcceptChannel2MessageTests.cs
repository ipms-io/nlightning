using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Managers;
using Common.Types;
using Utils;

public class AcceptChannel2MessageTests
{
    public AcceptChannel2MessageTests()
    {
        ConfigManager.Instance.DustLimitSatoshis = 1;
        ConfigManager.Instance.ToSelfDelay = 1;
        ConfigManager.Instance.HtlcMinimumMsat = 1000;
        ConfigManager.Instance.MinimumDepth = 3;
        ConfigManager.Instance.MaxHtlcValueInFlightMsat = 1000000;
        ConfigManager.Instance.MaxAcceptedHtlcs = 2;
        ConfigManager.Instance.Locktime = 1;
        ConfigManager.Instance.Network = Network.MAIN_NET;
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsAcceptChannel2Payload()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const ulong EXPECTED_FUNDING_SATOSHIS = 0;
        var expectedFundingPubKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var expectedRevocationBasepoint = new PubKey("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534".ToByteArray());
        var expectedPaymentBasePoint = new PubKey("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1".ToByteArray());
        var expectedDelayedPaymentBasepoint = new PubKey("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce".ToByteArray());
        var expectedHtlcBasepoint = new PubKey("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b".ToByteArray());
        var expectedFirstPerCommitmentPoint = new PubKey("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61".ToByteArray());

        var stream = new MemoryStream("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F61".ToByteArray());

        // Act
        var result = await AcceptChannel2Message.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChannelId, result.Payload.TemporaryChannelId);
        Assert.Equal(EXPECTED_FUNDING_SATOSHIS, result.Payload.FundingSatoshis);
        Assert.Equal(ConfigManager.Instance.DustLimitSatoshis, result.Payload.DustLimitSatoshis);
        Assert.Equal(ConfigManager.Instance.MaxHtlcValueInFlightMsat, result.Payload.MaxHtlcValueInFlightMsat);
        Assert.Equal(ConfigManager.Instance.HtlcMinimumMsat, result.Payload.HtlcMinimumMsat);
        Assert.Equal(ConfigManager.Instance.MinimumDepth, result.Payload.MinimumDepth);
        Assert.Equal(ConfigManager.Instance.ToSelfDelay, result.Payload.ToSelfDelay);
        Assert.Equal(ConfigManager.Instance.MaxAcceptedHtlcs, result.Payload.MaxAcceptedHtlcs);
        Assert.Equal(expectedFundingPubKey, result.Payload.FundingPubKey);
        Assert.Equal(expectedRevocationBasepoint, result.Payload.RevocationBasepoint);
        Assert.Equal(expectedPaymentBasePoint, result.Payload.PaymentBasepoint);
        Assert.Equal(expectedDelayedPaymentBasepoint, result.Payload.DelayedPaymentBasepoint);
        Assert.Equal(expectedHtlcBasepoint, result.Payload.HtlcBasepoint);
        Assert.Equal(expectedFirstPerCommitmentPoint, result.Payload.FirstPerCommitmentPoint);
        Assert.Null(result.Payload.AcceptTlvs);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_Then_ThrowsSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<SerializationException>(() => AcceptChannel2Payload.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const ulong FUNDING_SATOSHIS = 0;
        var fundingPubKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var revocationBasepoint = new PubKey("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534".ToByteArray());
        var paymentBasePoint = new PubKey("03A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C1".ToByteArray());
        var delayedPaymentBasepoint = new PubKey("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce".ToByteArray());
        var htlcBasepoint = new PubKey("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b".ToByteArray());
        var firstPerCommitmentPoint = new PubKey("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61".ToByteArray());

        var message = new AcceptChannel2Message(new AcceptChannel2Payload(channelId, FUNDING_SATOSHIS, fundingPubKey,
                                                revocationBasepoint, paymentBasePoint, delayedPaymentBasepoint,
                                                htlcBasepoint, firstPerCommitmentPoint));
        var stream = new MemoryStream();
        var expectedBytes = "004100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000F424000000000000003E8000000030001000202C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD653403A6BD98A33A52CD9D339EE20B4627AC60EC45C897E4FF182CC22ABA372C8D31C10280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F61".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}