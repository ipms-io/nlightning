namespace NLightning.Bolts.Tests.BOLT9;

using Bolts.BOLT9;
using Common.BitUtils;

public class FeaturesTests
{
    #region SetFeature IsFeatureSet
    [Theory]
    [InlineData(Feature.OptionDataLossProtect, false)]
    [InlineData(Feature.OptionDataLossProtect, true)]
    [InlineData(Feature.OptionUpfrontShutdownScript, false)]
    [InlineData(Feature.OptionUpfrontShutdownScript, true)]
    [InlineData(Feature.OptionSupportLargeChannel, false)]
    [InlineData(Feature.OptionSupportLargeChannel, true)]
    public void Given_Features_When_SetFeatureA_Then_OnlyFeatureAIsSet(Feature feature, bool isCompulsory)
    {
        // Arrange
        var features = new Features();

        // Act
        features.SetFeature(feature, isCompulsory);

        // Assert
        Assert.True(features.IsFeatureSet(feature, isCompulsory));
        Assert.False(features.IsFeatureSet(feature, !isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OptionPaymentMetadata, isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OptionPaymentMetadata, !isCompulsory));
    }

    [Theory]
    [InlineData(Feature.OptionDataLossProtect, false)]
    [InlineData(Feature.OptionDataLossProtect, true)]
    [InlineData(Feature.OptionUpfrontShutdownScript, false)]
    [InlineData(Feature.OptionUpfrontShutdownScript, true)]
    [InlineData(Feature.OptionSupportLargeChannel, false)]
    [InlineData(Feature.OptionSupportLargeChannel, true)]
    public void Given_Features_When_UnsetFeatureA_Then_FeatureBIsSet(Feature feature, bool isCompulsory)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(Feature.OptionPaymentMetadata, isCompulsory);
        features.SetFeature(feature, isCompulsory);

        // Act
        features.SetFeature(feature, isCompulsory, false);

        // Assert
        Assert.False(features.IsFeatureSet(feature, isCompulsory));
        Assert.False(features.IsFeatureSet(feature, !isCompulsory));
        Assert.True(features.IsFeatureSet(Feature.OptionPaymentMetadata, isCompulsory));
        Assert.False(features.IsFeatureSet(Feature.OptionPaymentMetadata, !isCompulsory));
    }

    [Theory]
    [InlineData(Feature.GossipQueriesEx, Feature.GossipQueries, false)]
    [InlineData(Feature.GossipQueriesEx, Feature.GossipQueries, true)]
    [InlineData(Feature.PaymentSecret, Feature.VarOnionOptin, false)]
    [InlineData(Feature.PaymentSecret, Feature.VarOnionOptin, true)]
    [InlineData(Feature.OptionAnchorOutputs, Feature.OptionStaticRemoteKey, false)]
    [InlineData(Feature.OptionAnchorOutputs, Feature.OptionStaticRemoteKey, true)]
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
    [InlineData(Feature.GossipQueries, Feature.GossipQueriesEx, false)]
    [InlineData(Feature.GossipQueries, Feature.GossipQueriesEx, true)]
    [InlineData(Feature.VarOnionOptin, Feature.PaymentSecret, false)]
    [InlineData(Feature.VarOnionOptin, Feature.PaymentSecret, true)]
    [InlineData(Feature.OptionStaticRemoteKey, Feature.OptionAnchorOutputs, false)]
    [InlineData(Feature.OptionStaticRemoteKey, Feature.OptionAnchorOutputs, true)]
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
    [InlineData(Feature.OptionDataLossProtect, false, false, false, false, true)]
    [InlineData(Feature.OptionDataLossProtect, false, true, false, false, true)]
    [InlineData(Feature.OptionDataLossProtect, false, true, false, true, true)]
    [InlineData(Feature.OptionDataLossProtect, false, false, false, true, true)]
    [InlineData(Feature.OptionDataLossProtect, true, false, false, false, true)]
    [InlineData(Feature.OptionDataLossProtect, false, false, true, false, true)]
    [InlineData(Feature.OptionDataLossProtect, true, false, true, false, true)]
    [InlineData(Feature.OptionDataLossProtect, false, true, true, false, false)]
    [InlineData(Feature.OptionDataLossProtect, true, false, false, true, false)]
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

        other.SetFeature(Feature.VarOnionOptin, true, false);
        other.SetFeature(Feature.VarOnionOptin, false, false);

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
        features.SetFeature(Feature.OptionDataLossProtect, false);
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
        features.SetFeature(Feature.OptionDataLossProtect, false);
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
        features.SetFeature(Feature.OptionZeroconf, false);
        var other = new Features();
        other.SetFeature(Feature.OptionZeroconf, false);
        other.SetFeature((int)Feature.OptionScidAlias, false);

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.False(result);
    }
    #endregion

    #region Serialization
    [Theory]
    [InlineData(Feature.OptionZeroconf, false, 7)]
    [InlineData(Feature.OptionZeroconf, true, 7)]
    [InlineData(Feature.OptionScidAlias, false, 6)]
    [InlineData(Feature.OptionScidAlias, true, 6)]
    [InlineData(Feature.OptionOnionMessages, false, 5)]
    [InlineData(Feature.OptionOnionMessages, true, 5)]
    [InlineData(Feature.OptionDualFund, false, 4)]
    [InlineData(Feature.OptionDualFund, true, 4)]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, false, 3)]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, true, 3)]
    [InlineData(Feature.OptionStaticRemoteKey, false, 2)]
    [InlineData(Feature.OptionStaticRemoteKey, true, 2)]
    [InlineData(Feature.GossipQueries, false, 1)]
    [InlineData(Feature.GossipQueries, true, 1)]
    public async Task Given_Features_When_Serialize_Then_BytesAreTrimmed(Feature feature, bool isCompulsory, int expectedLength)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VarOnionOptin, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream);
        var bytes = stream.ToArray();
        var length = EndianBitConverter.ToUInt16BE(bytes[..2]);

        // Assert
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(Feature.OptionZeroconf, false, 7)]
    [InlineData(Feature.OptionZeroconf, true, 7)]
    [InlineData(Feature.OptionScidAlias, false, 6)]
    [InlineData(Feature.OptionScidAlias, true, 6)]
    [InlineData(Feature.OptionOnionMessages, false, 5)]
    [InlineData(Feature.OptionOnionMessages, true, 5)]
    [InlineData(Feature.OptionDualFund, false, 4)]
    [InlineData(Feature.OptionDualFund, true, 4)]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, false, 3)]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, true, 3)]
    [InlineData(Feature.OptionStaticRemoteKey, false, 2)]
    [InlineData(Feature.OptionStaticRemoteKey, true, 2)]
    [InlineData(Feature.GossipQueries, false, 1)]
    [InlineData(Feature.GossipQueries, true, 1)]
    public async Task Given_Features_When_SerializeWithoutLength_Then_LengthIsKnown(Feature feature, bool isCompulsory, int expectedLength)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VarOnionOptin, false, false);

        using var stream = new MemoryStream();

        // Act
        await features.SerializeAsync(stream, includeLength: false);
        var bytes = stream.ToArray();

        // Assert
        Assert.Equal(expectedLength, bytes.Length);
    }

    [Theory]
    [InlineData(Feature.OptionZeroconf, false, new byte[7] { 8, 128, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionZeroconf, true, new byte[7] { 4, 64, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionScidAlias, false, new byte[6] { 128, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionScidAlias, true, new byte[6] { 64, 0, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionOnionMessages, false, new byte[5] { 128, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionOnionMessages, true, new byte[5] { 64, 0, 0, 0, 0 })]
    [InlineData(Feature.OptionDualFund, false, new byte[4] { 32, 0, 0, 0 })]
    [InlineData(Feature.OptionDualFund, true, new byte[4] { 16, 0, 0, 0 })]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, false, new byte[3] { 128, 32, 0 })]
    [InlineData(Feature.OptionAnchorsZeroFeeHtlcTx, true, new byte[3] { 64, 16, 0 })]
    [InlineData(Feature.OptionStaticRemoteKey, false, new byte[2] { 32, 0 })]
    [InlineData(Feature.OptionStaticRemoteKey, true, new byte[2] { 16, 0 })]
    [InlineData(Feature.GossipQueries, false, new byte[1] { 128 })]
    [InlineData(Feature.GossipQueries, true, new byte[1] { 64 })]
    public async Task Given_Features_When_Serialize_Then_BytesAreKnown(Feature feature, bool isCompulsory, byte[] expected)
    {
        // Arrange
        var features = new Features();
        features.SetFeature(feature, isCompulsory);
        // Clean default features
        features.SetFeature(Feature.VarOnionOptin, false, false);

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
        features.SetFeature(Feature.OptionDataLossProtect, true);
        // Clean default features
        features.SetFeature(Feature.VarOnionOptin, false, false);

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
        features.SetFeature(Feature.OptionSupportLargeChannel, true);
        // Clean default features
        features.SetFeature(Feature.VarOnionOptin, false, false);

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
    [InlineData(new byte[9] { 0, 7, 8, 128, 0, 0, 0, 0, 0 }, false, Feature.OptionZeroconf)]
    [InlineData(new byte[9] { 0, 7, 4, 64, 0, 0, 0, 0, 0 }, true, Feature.OptionZeroconf)]
    [InlineData(new byte[8] { 0, 6, 128, 0, 0, 0, 0, 0 }, false, Feature.OptionScidAlias)]
    [InlineData(new byte[8] { 0, 6, 64, 0, 0, 0, 0, 0 }, true, Feature.OptionScidAlias)]
    [InlineData(new byte[7] { 0, 5, 128, 0, 0, 0, 0 }, false, Feature.OptionOnionMessages)]
    [InlineData(new byte[7] { 0, 5, 64, 0, 0, 0, 0 }, true, Feature.OptionOnionMessages)]
    [InlineData(new byte[6] { 0, 4, 32, 0, 0, 0 }, false, Feature.OptionDualFund)]
    [InlineData(new byte[6] { 0, 4, 16, 0, 0, 0 }, true, Feature.OptionDualFund)]
    [InlineData(new byte[5] { 0, 3, 128, 32, 0 }, false, Feature.OptionAnchorsZeroFeeHtlcTx)]
    [InlineData(new byte[5] { 0, 3, 64, 16, 0 }, true, Feature.OptionAnchorsZeroFeeHtlcTx)]
    [InlineData(new byte[4] { 0, 2, 32, 0 }, false, Feature.OptionStaticRemoteKey)]
    [InlineData(new byte[4] { 0, 2, 16, 0 }, true, Feature.OptionStaticRemoteKey)]
    [InlineData(new byte[3] { 0, 1, 128 }, false, Feature.GossipQueries)]
    [InlineData(new byte[3] { 0, 1, 64 }, true, Feature.GossipQueries)]
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
        Assert.True(features.IsFeatureSet(Feature.OptionDataLossProtect, true));
        Assert.False(features.IsFeatureSet(Feature.OptionDataLossProtect, false));
    }

    [Fact]
    public async Task Given_WrongLengthBuffer_When_Deserialize_Then_FeatureIsNotSet()
    {
        // Arrange
        using var stream = new MemoryStream([0, 6, 8, 128, 0, 0, 0, 0, 0]);

        // Act
        var features = await Features.DeserializeAsync(stream);

        // Assert
        Assert.False(features.IsFeatureSet(Feature.OptionZeroconf, false));
        Assert.False(features.IsFeatureSet(Feature.OptionZeroconf, true));
    }
    #endregion

    #region Combine
    [Fact]
    public void Given_Features_When_Combine_Then_FeaturesAreCombined()
    {
        // Arrange
        var global = new Features();
        global.SetFeature(Feature.OptionDataLossProtect, true);
        global.SetFeature(Feature.OptionUpfrontShutdownScript, false);
        global.SetFeature(Feature.GossipQueries, true);

        var features = new Features();
        features.SetFeature(Feature.OptionUpfrontShutdownScript, true);
        features.SetFeature(Feature.GossipQueries, false);
        features.SetFeature(Feature.OptionSupportLargeChannel, true);

        // Act
        var combined = Features.Combine(global, features);

        // Assert
        Assert.True(combined.IsFeatureSet(Feature.OptionDataLossProtect, true));
        Assert.True(combined.IsFeatureSet(Feature.OptionUpfrontShutdownScript, true));
        Assert.True(combined.IsFeatureSet(Feature.OptionSupportLargeChannel, true));
        Assert.True(combined.IsFeatureSet(Feature.GossipQueries, true));
    }
    #endregion
}