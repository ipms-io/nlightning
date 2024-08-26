namespace NLightning.Bolts.Tests.BOLT9;

using Bolts.BOLT9;
using Common.BitUtils;

public class FeaturesTests
{
    #region SetFeature IsFeatureSet
    [Theory]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, true)]
    [InlineData(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, false)]
    [InlineData(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, true)]
    [InlineData(Feature.OPTION_SUPPORT_LARGE_CHANNEL, false)]
    [InlineData(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true)]
    public void Given_Features_When_SetFeatureA_Then_OnlyFeatureAIsSet(Feature feature, bool isCompulsory)
    {
        // Arrange
        var features = new Features();

        // Act
        features.SetFeature(feature, isCompulsory);

        // Assert
        Assert.True(features.IsFeatureSet(feature, isCompulsory));
        Assert.False(features.IsFeatureSet(feature, !isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, !isCompulsory));
    }

    [Theory]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, true)]
    [InlineData(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, false)]
    [InlineData(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, true)]
    [InlineData(Feature.OPTION_SUPPORT_LARGE_CHANNEL, false)]
    [InlineData(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true)]
    public void Given_Features_When_UnsetFeatureA_Then_FeatureBIsSet(Feature feature, bool isCompulsory)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(Feature.OPTION_PAYMENT_METADATA, isCompulsory);
        features.SetFeature(feature, isCompulsory);

        // Act
        features.SetFeature(feature, isCompulsory, false);

        // Assert
        Assert.False(features.IsFeatureSet(feature, isCompulsory));
        Assert.False(features.IsFeatureSet(feature, !isCompulsory));
        Assert.True(features.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OPTION_PAYMENT_METADATA, !isCompulsory));
    }

    [Theory]
    [InlineData(Feature.GOSSIP_QUERIES_EX, Feature.GOSSIP_QUERIES, false)]
    [InlineData(Feature.GOSSIP_QUERIES_EX, Feature.GOSSIP_QUERIES, true)]
    [InlineData(Feature.PAYMENT_SECRET, Feature.VAR_ONION_OPTIN, false)]
    [InlineData(Feature.PAYMENT_SECRET, Feature.VAR_ONION_OPTIN, true)]
    [InlineData(Feature.OPTION_ANCHOR_OUTPUTS, Feature.OPTION_STATIC_REMOTE_KEY, false)]
    [InlineData(Feature.OPTION_ANCHOR_OUTPUTS, Feature.OPTION_STATIC_REMOTE_KEY, true)]
    public void Given_Features_When_SetFeatureADependsOnFeatureB_Then_FeatureBIsSet(Feature feature, Feature dependsOn, bool isCompulsory)
    {
        // Arrange
        var features = new Features();

        // Act
        features.SetFeature(feature, isCompulsory);

        // Assert
        Assert.True(features.IsFeatureSet(feature, isCompulsory));
        Assert.True(features.IsFeatureSet(dependsOn, isCompulsory));
    }

    [Theory]
    [InlineData(Feature.GOSSIP_QUERIES, Feature.GOSSIP_QUERIES_EX, false)]
    [InlineData(Feature.GOSSIP_QUERIES, Feature.GOSSIP_QUERIES_EX, true)]
    [InlineData(Feature.VAR_ONION_OPTIN, Feature.PAYMENT_SECRET, false)]
    [InlineData(Feature.VAR_ONION_OPTIN, Feature.PAYMENT_SECRET, true)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, Feature.OPTION_ANCHOR_OUTPUTS, false)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, Feature.OPTION_ANCHOR_OUTPUTS, true)]
    public void Given_Features_When_UnsetFeatureA_Then_FeatureBIsUnset(Feature feature, Feature dependent, bool isCompulsory)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(dependent, isCompulsory);

        // Act
        features.SetFeature(feature, isCompulsory, false);

        // Assert
        Assert.False(features.IsFeatureSet(feature, isCompulsory));
        Assert.False(features.IsFeatureSet(dependent, isCompulsory));
    }

    [Fact]
    public void Given_Features_When_SetUnknownFeature_Then_UnknownFeatureIsSet()
    {
        // Arrange
        var features = new Features();

        // Act
        features.SetFeature(42, true);

        // Assert
        Assert.True(features.IsFeatureSet(42, false));
    }
    #endregion

    #region IsCompatible
    [Theory]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, false, false, false, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, true, false, false, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, true, false, true, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, false, false, true, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, true, false, false, false, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, false, true, false, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, true, false, true, false, true)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, false, true, true, false, false)]
    [InlineData(Feature.OPTION_DATA_LOSS_PROTECT, true, false, false, true, false)]
    public void Given_Features_When_IsCompatible_Then_ReturnIsKnown(Feature feature, bool unsetLocal, bool isLocalCompulsorySet, bool unsetOther, bool isOtherCompulsorySet, bool expected)
    {
        // Arrange
        var features = new Features();
        var other = new Features();
        if (unsetLocal)
        {
            features.SetFeature(feature, isLocalCompulsorySet, false);
            features.SetFeature(feature, !isLocalCompulsorySet, false);
            other.SetFeature(feature, isOtherCompulsorySet);
        }
        else if (unsetOther)
        {
            other.SetFeature(feature, isOtherCompulsorySet, false);
            other.SetFeature(feature, !isOtherCompulsorySet, false);
            features.SetFeature(feature, isLocalCompulsorySet);
        }
        else
        {
            features.SetFeature(feature, isLocalCompulsorySet);
            other.SetFeature(feature, isOtherCompulsorySet);
        }

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Given_Features_When_OtherDontSupportVarOnionOptin_Then_ReturnFalse()
    {
        // Arrange
        var features = new Features();
        var other = new Features();

        other.SetFeature(Feature.VAR_ONION_OPTIN, true, false);
        other.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Given_Features_When_OtherFeatureHasUnknownOptionalFeatureSet_Then_ReturnTrue()
    {
        // Arrange
        var features = new Features();
        var other = new Features();
        features.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, false);
        other.SetFeature(41, true);

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Given_Features_When_OtherFeatureHasUnknownCompulsoryFeatureSet_Then_ReturnFalse()
    {
        // Arrange
        var features = new Features();
        var other = new Features();
        features.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, false);
        other.SetFeature(42, true);

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Given_Features_When_OtherFeatureDontSetDependency_Then_ReturnFalse()
    {
        // Arrange
        var features = new Features();
        features.SetFeature(Feature.OPTION_ZEROCONF, false);
        var other = new Features();
        other.SetFeature(Feature.OPTION_ZEROCONF, false);
        other.SetFeature((int)Feature.OPTION_SCID_ALIAS, false);

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.False(result);
    }
    #endregion

    #region Serialization
    [Theory]
    [InlineData(Feature.OPTION_ZEROCONF, false, 7)]
    [InlineData(Feature.OPTION_ZEROCONF, true, 7)]
    [InlineData(Feature.OPTION_SCID_ALIAS, false, 6)]
    [InlineData(Feature.OPTION_SCID_ALIAS, true, 6)]
    [InlineData(Feature.OPTION_ONION_MESSAGES, false, 5)]
    [InlineData(Feature.OPTION_ONION_MESSAGES, true, 5)]
    [InlineData(Feature.OPTION_DUAL_FUND, false, 4)]
    [InlineData(Feature.OPTION_DUAL_FUND, true, 4)]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false, 3)]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, true, 3)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, false, 2)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, true, 2)]
    [InlineData(Feature.GOSSIP_QUERIES, false, 1)]
    [InlineData(Feature.GOSSIP_QUERIES, true, 1)]
    public async Task Given_Features_When_Serialize_Then_BytesAreTrimmed(Feature feature, bool isCompulsory, int expectedLength)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream);
        var bytes = stream.ToArray();
        var length = EndianBitConverter.ToUInt16BigEndian(bytes[..2]);

        // Assert
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(Feature.OPTION_ZEROCONF, false, 7)]
    [InlineData(Feature.OPTION_ZEROCONF, true, 7)]
    [InlineData(Feature.OPTION_SCID_ALIAS, false, 6)]
    [InlineData(Feature.OPTION_SCID_ALIAS, true, 6)]
    [InlineData(Feature.OPTION_ONION_MESSAGES, false, 5)]
    [InlineData(Feature.OPTION_ONION_MESSAGES, true, 5)]
    [InlineData(Feature.OPTION_DUAL_FUND, false, 4)]
    [InlineData(Feature.OPTION_DUAL_FUND, true, 4)]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false, 3)]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, true, 3)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, false, 2)]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, true, 2)]
    [InlineData(Feature.GOSSIP_QUERIES, false, 1)]
    [InlineData(Feature.GOSSIP_QUERIES, true, 1)]
    public async Task Given_Features_When_SerializeWithoutLength_Then_LengthIsKnown(Feature feature, bool isCompulsory, int expectedLength)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream, includeLength: false);
        var bytes = stream.ToArray();

        // Assert
        Assert.Equal(expectedLength, bytes.Length);
    }

    [Theory]
    [InlineData(Feature.OPTION_ZEROCONF, false, new byte[] { 8, 128, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_ZEROCONF, true, new byte[] { 4, 64, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_SCID_ALIAS, false, new byte[] { 128, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_SCID_ALIAS, true, new byte[] { 64, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_ONION_MESSAGES, false, new byte[] { 128, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_ONION_MESSAGES, true, new byte[] { 64, 0, 0, 0, 0 })]
    [InlineData(Feature.OPTION_DUAL_FUND, false, new byte[] { 32, 0, 0, 0 })]
    [InlineData(Feature.OPTION_DUAL_FUND, true, new byte[] { 16, 0, 0, 0 })]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false, new byte[] { 128, 32, 0 })]
    [InlineData(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, true, new byte[] { 64, 16, 0 })]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, false, new byte[] { 32, 0 })]
    [InlineData(Feature.OPTION_STATIC_REMOTE_KEY, true, new byte[] { 16, 0 })]
    [InlineData(Feature.GOSSIP_QUERIES, false, new byte[] { 128 })]
    [InlineData(Feature.GOSSIP_QUERIES, true, new byte[] { 64 })]
    public async Task Given_Features_When_Serialize_Then_BytesAreKnown(Feature feature, bool isCompulsory, byte[] expected)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream);
        var bytes = stream.ToArray();

        // Assert
        Assert.Equal(expected, bytes[2..]);
    }

    [Fact]
    public async Task Given_Features_When_SerializeWithoutLength_Then_BytesAreKnown()
    {
        // Arrange
        var features = new Features();
        // Sets bit 0
        features.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, true);
        // Clean default features
        features.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream, includeLength: false);
        var bytes = stream.ToArray();

        // Assert
        Assert.Equal([1], bytes);
    }

    [Fact]
    public async Task Given_Features_When_SerializeAsGlobal_Then_NoFeaturesGreaterThan13ArePresent()
    {
        // Arrange
        var features = new Features();
        // Sets bit 0
        features.SetFeature(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true);
        // Clean default features
        features.SetFeature(Feature.VAR_ONION_OPTIN, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream, true);
        var bytes = stream.ToArray();

        // Assert
        Assert.Equal(2, bytes.Length);
    }
    #endregion

    #region Deserialization
    [Theory]
    [InlineData(new byte[] { 0, 7, 8, 128, 0, 0, 0, 0, 0 }, false, Feature.OPTION_ZEROCONF)]
    [InlineData(new byte[] { 0, 7, 4, 64, 0, 0, 0, 0, 0 }, true, Feature.OPTION_ZEROCONF)]
    [InlineData(new byte[] { 0, 6, 128, 0, 0, 0, 0, 0 }, false, Feature.OPTION_SCID_ALIAS)]
    [InlineData(new byte[] { 0, 6, 64, 0, 0, 0, 0, 0 }, true, Feature.OPTION_SCID_ALIAS)]
    [InlineData(new byte[] { 0, 5, 128, 0, 0, 0, 0 }, false, Feature.OPTION_ONION_MESSAGES)]
    [InlineData(new byte[] { 0, 5, 64, 0, 0, 0, 0 }, true, Feature.OPTION_ONION_MESSAGES)]
    [InlineData(new byte[] { 0, 4, 32, 0, 0, 0 }, false, Feature.OPTION_DUAL_FUND)]
    [InlineData(new byte[] { 0, 4, 16, 0, 0, 0 }, true, Feature.OPTION_DUAL_FUND)]
    [InlineData(new byte[] { 0, 3, 128, 32, 0 }, false, Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX)]
    [InlineData(new byte[] { 0, 3, 64, 16, 0 }, true, Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX)]
    [InlineData(new byte[] { 0, 2, 32, 0 }, false, Feature.OPTION_STATIC_REMOTE_KEY)]
    [InlineData(new byte[] { 0, 2, 16, 0 }, true, Feature.OPTION_STATIC_REMOTE_KEY)]
    [InlineData(new byte[] { 0, 1, 128 }, false, Feature.GOSSIP_QUERIES)]
    [InlineData(new byte[] { 0, 1, 64 }, true, Feature.GOSSIP_QUERIES)]
    public async Task Given_Buffer_When_Deserialize_Then_FeatureIsSet(byte[] buffer, bool isCompulsory, Feature expected)
    {
        // Arrange
        using var stream = new MemoryStream(buffer);

        // Act
        var features = await Features.DeserializeAsync(stream);

        // Assert
        Assert.True(features.IsFeatureSet(expected, isCompulsory));
        Assert.False(features.IsFeatureSet(expected, !isCompulsory));
    }

    [Fact]
    public async Task Given_Buffer_When_DeserializeWithoutLength_Then_FeatureIsSet()
    {
        // Arrange
        using var stream = new MemoryStream([0, 0, 0, 0, 0, 0, 0, 1]);

        // Act
        var features = await Features.DeserializeAsync(stream, false);

        // Assert
        Assert.True(features.IsFeatureSet(Feature.OPTION_DATA_LOSS_PROTECT, true));
        Assert.False(features.IsFeatureSet(Feature.OPTION_DATA_LOSS_PROTECT, false));
    }

    [Fact]
    public async Task Given_WrongLengthBuffer_When_Deserialize_Then_FeatureIsNotSet()
    {
        // Arrange
        using var stream = new MemoryStream([0, 6, 8, 128, 0, 0, 0, 0, 0]);

        // Act
        var features = await Features.DeserializeAsync(stream);

        // Assert
        Assert.False(features.IsFeatureSet(Feature.OPTION_ZEROCONF, false));
        Assert.False(features.IsFeatureSet(Feature.OPTION_ZEROCONF, true));
    }
    #endregion

    #region Combine
    [Fact]
    public void Given_Features_When_Combine_Then_FeaturesAreCombined()
    {
        // Arrange
        var global = new Features();
        global.SetFeature(Feature.OPTION_DATA_LOSS_PROTECT, true);
        global.SetFeature(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, false);
        global.SetFeature(Feature.GOSSIP_QUERIES, true);

        var features = new Features();
        features.SetFeature(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, true);
        features.SetFeature(Feature.GOSSIP_QUERIES, false);
        features.SetFeature(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true);

        // Act
        var combined = Features.Combine(global, features);

        // Assert
        Assert.True(combined.IsFeatureSet(Feature.OPTION_DATA_LOSS_PROTECT, true));
        Assert.True(combined.IsFeatureSet(Feature.OPTION_UPFRONT_SHUTDOWN_SCRIPT, true));
        Assert.True(combined.IsFeatureSet(Feature.OPTION_SUPPORT_LARGE_CHANNEL, true));
        Assert.True(combined.IsFeatureSet(Feature.GOSSIP_QUERIES, true));
    }
    #endregion
}