using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Managers;
using Common.Types;
using Utils;

public class OpenChannel2MessageTests
{
    public OpenChannel2MessageTests()
    {
        ConfigManager.Instance.DustLimitSatoshis = 1;
        ConfigManager.Instance.ToSelfDelay = 1;
        ConfigManager.Instance.HtlcMinimumMsat = 1000;
        ConfigManager.Instance.MaxHtlcValueInFlightMsat = 1000000;
        ConfigManager.Instance.MaxAcceptedHtlcs = 2;
        ConfigManager.Instance.Locktime = 1;
        ConfigManager.Instance.Network = Network.MAIN_NET;
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsOpenChannel2Payload()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        const uint EXPECTED_FUNDING_FEERATE = 1000;
        const uint EXPECTED_COMMITMENT_FEERATE = 2000;
        const ulong EXPECTED_FUNDING_SATOSHIS = 100000;
        var expectedFundingPubKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var expectedRevocationBasepoint = new PubKey("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534".ToByteArray());
        var expectedDelayedPaymentBasepoint = new PubKey("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce".ToByteArray());
        var expectedHtlcBasepoint = new PubKey("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b".ToByteArray());
        var expectedFirstPerCommitmentPoint = new PubKey("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61".ToByteArray());
        var expectedSecondPerCommitmentPoint = new PubKey("03a92b07cbae641dcfd482825233aecc2d5012913b48040131db3222670c2bffcd".ToByteArray());
        var expectedChannelFlags = new ChannelFlags();

        var stream = new MemoryStream("6FE28C0AB6F1B372C1A6A246AE63F74F931E8365E15A089C68D61900000000000000000000000000000000000000000000000000000000000000000000000000000003E8000007D000000000000186A0000000000000000100000000000F424000000000000003E8000100020000000102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD65340280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F6103A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD00".ToByteArray());

        // Act
        var result = await OpenChannel2Message.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConfigManager.Instance.Network.ChainHash, result.Payload.ChainHash);
        Assert.Equal(expectedChannelId, result.Payload.ChannelId);
        Assert.Equal(EXPECTED_FUNDING_FEERATE, result.Payload.FundingFeeRatePerKw);
        Assert.Equal(EXPECTED_COMMITMENT_FEERATE, result.Payload.CommitmentFeeRatePerKw);
        Assert.Equal(EXPECTED_FUNDING_SATOSHIS, result.Payload.FundingSatoshis);
        Assert.Equal(ConfigManager.Instance.DustLimitSatoshis, result.Payload.DustLimitSatoshis);
        Assert.Equal(ConfigManager.Instance.MaxHtlcValueInFlightMsat, result.Payload.MaxHtlcValueInFlightMsat);
        Assert.Equal(ConfigManager.Instance.HtlcMinimumMsat, result.Payload.HtlcMinimumMsat);
        Assert.Equal(ConfigManager.Instance.ToSelfDelay, result.Payload.ToSelfDelay);
        Assert.Equal(ConfigManager.Instance.MaxAcceptedHtlcs, result.Payload.MaxAcceptedHtlcs);
        Assert.Equal(ConfigManager.Instance.Locktime, result.Payload.Locktime);
        Assert.Equal(expectedFundingPubKey, result.Payload.FundingPubKey);
        Assert.Equal(expectedRevocationBasepoint, result.Payload.RevocationBasepoint);
        Assert.Equal(expectedDelayedPaymentBasepoint, result.Payload.DelayedPaymentBasepoint);
        Assert.Equal(expectedHtlcBasepoint, result.Payload.HtlcBasepoint);
        Assert.Equal(expectedFirstPerCommitmentPoint, result.Payload.FirstPerCommitmentPoint);
        Assert.Equal(expectedSecondPerCommitmentPoint, result.Payload.SecondPerCommitmentPoint);
        Assert.Equal(expectedChannelFlags, result.Payload.ChannelFlags);
        Assert.Null(result.Payload.OpeningTlvs);
    }

    [Fact]
    public async Task Given_InvalidStream_When_DeserializeAsync_Then_ThrowsSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream([0x00, 0x01, 0x02]);

        // Act & Assert
        await Assert.ThrowsAsync<SerializationException>(() => OpenChannel2Payload.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        const uint FUNDING_FEERATE = 1000;
        const uint COMMITMENT_FEERATE = 2000;
        const ulong FUNDING_SATOSHIS = 100000;
        var fundingPubKey = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var revocationBasepoint = new PubKey("0315525220b88467a0ee3a111ae49ffdc337136ef51031cfc1c9883b7d1cbd6534".ToByteArray());
        var delayedPaymentBasepoint = new PubKey("0280a3001fe999b1fe9842317ce29f71b9bb5888448a2cf5e115bfc808ba4568ce".ToByteArray());
        var htlcBasepoint = new PubKey("03798e7efc8c950fcd6c9e3af4bbad16a26f14c838e99651f637ddd73ddc88531b".ToByteArray());
        var firstPerCommitmentPoint = new PubKey("0326550f5ae41511e767afe0a9c7e20a73174875a6d1ee4e9e128cbb1fb0099f61".ToByteArray());
        var secondPerCommitmentPoint = new PubKey("03a92b07cbae641dcfd482825233aecc2d5012913b48040131db3222670c2bffcd".ToByteArray());
        var channelFlags = new ChannelFlags();

        var message = new OpenChannel2Message(new OpenChannel2Payload(channelId, FUNDING_FEERATE, COMMITMENT_FEERATE, FUNDING_SATOSHIS,
                                              fundingPubKey, revocationBasepoint, delayedPaymentBasepoint, htlcBasepoint,
                                              firstPerCommitmentPoint, secondPerCommitmentPoint, channelFlags));
        var stream = new MemoryStream();
        var expectedBytes = "00406FE28C0AB6F1B372C1A6A246AE63F74F931E8365E15A089C68D61900000000000000000000000000000000000000000000000000000000000000000000000000000003E8000007D000000000000186A0000000000000000100000000000F424000000000000003E8000100020000000102C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750315525220B88467A0EE3A111AE49FFDC337136EF51031CFC1C9883B7D1CBD65340280A3001FE999B1FE9842317CE29F71B9BB5888448A2CF5E115BFC808BA4568CE03798E7EFC8C950FCD6C9E3AF4BBAD16A26F14C838E99651F637DDD73DDC88531B0326550F5AE41511E767AFE0A9C7E20A73174875A6D1EE4E9E128CBB1FB0099F6103A92B07CBAE641DCFD482825233AECC2D5012913B48040131DB3222670C2BFFCD00".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}