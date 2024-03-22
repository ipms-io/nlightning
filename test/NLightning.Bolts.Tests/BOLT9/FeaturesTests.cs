namespace NLightning.Bolts.Tests.BOLT9;

using Bolts.BOLT9;

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

    [Theory]
    [InlineData(false, true, false, true, true)]
    [InlineData(false, false, false, true, true)]
    [InlineData(false, true, false, false, true)]
    [InlineData(false, false, false, false, true)]
    [InlineData(true, false, false, true, false)]
    [InlineData(true, true, false, false, false)]
    [InlineData(false, true, true, false, false)]
    [InlineData(false, false, true, false, false)]
    public void Given_VarOnionOptin_When_IsCompatible_Then_ResultIsKnown(bool unsetLocal, bool isLocalCompulsorySet, bool unsetOther, bool isOtherCompulsorySet, bool expected)
    {
        // Arrange
        var features = new Features();
        var other = new Features();
        if (unsetLocal)
        {
            features.SetFeature(Feature.VarOnionOptin, isLocalCompulsorySet, false);
            features.SetFeature(Feature.VarOnionOptin, !isLocalCompulsorySet, false);
            other.SetFeature(Feature.VarOnionOptin, isOtherCompulsorySet);
        }
        else if (unsetOther)
        {
            other.SetFeature(Feature.VarOnionOptin, isOtherCompulsorySet, false);
            other.SetFeature(Feature.VarOnionOptin, !isOtherCompulsorySet, false);
            features.SetFeature(Feature.VarOnionOptin, isLocalCompulsorySet);
        }
        else
        {
            features.SetFeature(Feature.VarOnionOptin, isLocalCompulsorySet);
            other.SetFeature(Feature.VarOnionOptin, isOtherCompulsorySet);
        }

        // Act
        var result = features.IsCompatible(other);

        // Assert
        Assert.Equal(expected, result);
    }
    #endregion
}