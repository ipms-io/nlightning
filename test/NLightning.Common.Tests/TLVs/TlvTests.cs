using NBitcoin;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Common.Tests.TLVs;

using Common.Types;

public class BaseTlvTests
{
    [Fact]
    public async Task BlindedPathTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var pubKey = new PubKey("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
        var blindedPathTlv = new BlindedPathTlv(pubKey);

        // Act
        using var stream = new MemoryStream();
        await blindedPathTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = BlindedPathTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(blindedPathTlv.PathKey, deserializedTlv.PathKey);
    }

    [Fact]
    public async Task ChannelTypeTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var channelType = new byte[] { 0x01, 0x02, 0x03 };
        var channelTypeTlv = new ChannelTypeTlv(channelType);

        // Act
        using var stream = new MemoryStream();
        await channelTypeTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = ChannelTypeTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(channelTypeTlv.ChannelType, deserializedTlv.ChannelType);
    }

    [Fact]
    public async Task FeeRangeTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var minFee = LightningMoney.Satoshis(1000);
        var maxFee = LightningMoney.Satoshis(2000);
        var feeRangeTlv = new FeeRangeTlv(minFee, maxFee);

        // Act
        using var stream = new MemoryStream();
        await feeRangeTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = FeeRangeTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(feeRangeTlv.MinFeeAmount, deserializedTlv.MinFeeAmount);
        Assert.Equal(feeRangeTlv.MaxFeeAmount, deserializedTlv.MaxFeeAmount);
    }

    [Fact]
    public async Task FundingOutputContributionTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var satoshis = 5000L;
        var fundingTlv = new FundingOutputContributionTlv(satoshis);

        // Act
        using var stream = new MemoryStream();
        await fundingTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = FundingOutputContributionTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(fundingTlv.Satoshis, deserializedTlv.Satoshis);
    }

    [Fact]
    public async Task NetworksTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var chainHashes = new[] { new ChainHash(new byte[32]), new ChainHash(new byte[32]) };
        var networksTlv = new NetworksTlv(chainHashes);

        // Act
        using var stream = new MemoryStream();
        await networksTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = NetworksTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(networksTlv.ChainHashes?.Count(), deserializedTlv.ChainHashes?.Count());
    }

    [Fact]
    public async Task RequireConfirmedInputsTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();

        // Act
        using var stream = new MemoryStream();
        await requireConfirmedInputsTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = RequireConfirmedInputsTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.NotNull(deserializedTlv);
    }

    [Fact]
    public async Task NextFundingTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var nextFundingTxId = new byte[32];
        var nextFundingTlv = new NextFundingTlv(nextFundingTxId);

        // Act
        using var stream = new MemoryStream();
        await nextFundingTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = NextFundingTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(nextFundingTlv.NextFundingTxId, deserializedTlv.NextFundingTxId);
    }

    [Fact]
    public async Task ShortChannelIdTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var shortChannelId = new ShortChannelId(new byte[8]);
        var shortChannelIdTlv = new ShortChannelIdTlv(shortChannelId);

        // Act
        using var stream = new MemoryStream();
        await shortChannelIdTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = ShortChannelIdTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(shortChannelIdTlv.ShortChannelId, deserializedTlv.ShortChannelId);
    }

    [Fact]
    public async Task UpfrontShutdownScriptTlv_SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var script = new Script(new byte[] { 0x01, 0x02, 0x03 });
        var upfrontShutdownScriptTlv = new UpfrontShutdownScriptTlv(script);

        // Act
        using var stream = new MemoryStream();
        await upfrontShutdownScriptTlv.SerializeAsync(stream);
        stream.Position = 0;
        var deserializedTlv = UpfrontShutdownScriptTlv.FromTlv(await Tlv.DeserializeAsync(stream));

        // Assert
        Assert.Equal(upfrontShutdownScriptTlv.ShutdownScriptPubkey, deserializedTlv.ShutdownScriptPubkey);
    }
}