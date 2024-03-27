namespace NLightning.Bolts.BOLT9;

/// <summary>
/// Represents the features supported by a node. <see href="https://github.com/lightning/bolts/blob/master/09-features.md">BOLT-9</see>
/// </summary>
public class Features
{
    /// <summary>
    /// Some features are dependent on other features. This dictionary contains the dependencies.
    /// </summary>
    private static readonly Dictionary<Feature, Feature[]> s_featureDependencies = new()
    {
        // This   \/                          Depends on this \/
        { Feature.GossipQueriesEx,            new[] { Feature.GossipQueries } },
        { Feature.PaymentSecret,              new[] { Feature.VarOnionOptin } },
        { Feature.BasicMpp,                   new[] { Feature.PaymentSecret } },
        { Feature.OptionAnchorOutputs,        new[] { Feature.OptionStaticRemoteKey } },
        { Feature.OptionAnchorsZeroFeeHtlcTx, new[] { Feature.OptionStaticRemoteKey } },
        { Feature.OptionRouteBlinding,        new[] { Feature.VarOnionOptin } },
        { Feature.OptionZeroconf,             new[] { Feature.OptionScidAlias } },
    };

    private ulong _featureFlags;

    /// <summary>
    /// Initializes a new instance of the <see cref="Features"/> class.
    /// </summary>
    /// <remarks>
    /// Allways set the bit of <see cref="Feature.VarOnionOptin"/> as Optional.
    /// </remarks>
    public Features()
    {
        // Allways set the compulsory bit of var_onion_optin
        SetFeature(Feature.VarOnionOptin, false);
    }

    /// <summary>
    /// Sets a feature.
    /// </summary>
    /// <param name="feature">The feature to set.</param>
    /// <param name="isCompulsory">If the feature is compulsory.</param>
    /// <param name="isSet">true to set the feature, false to unset it</param>
    /// <remarks>
    /// If the feature has dependencies, they will be set first.
    /// The denpendencies keeps the same isCompulsory value as the feature being set.
    /// </remarks>
    public void SetFeature(Feature feature, bool isCompulsory, bool isSet = true)
    {
        // If we're setting the feature and it has dependencies, set them first
        if (isSet)
        {
            if (s_featureDependencies.TryGetValue(feature, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    SetFeature(dependency, isCompulsory, isSet);
                }
            }
        }
        else // If we're unsetting the feature and it has dependents, unset them first
        {
            foreach (var dependent in s_featureDependencies.Where(x => x.Value.Contains(feature)).Select(x => x.Key))
            {
                SetFeature(dependent, isCompulsory, isSet);
            }
        }

        var bitPosition = (int)feature;

        if (isCompulsory)
        {
            // Unset the non-compulsory bit
            SetFeature(bitPosition, false);
            --bitPosition;
        }
        else
        {
            // Unset the compulsory bit
            SetFeature(bitPosition - 1, false);
        }

        // Then set the feature itself
        SetFeature(bitPosition, isSet);
    }
    /// <summary>
    /// Sets a feature.
    /// </summary>
    /// <param name="bitPosition">The bit position of the feature to set.</param>
    /// <param name="isSet">true to set the feature, false to unset it</param>
    public void SetFeature(int bitPosition, bool isSet)
    {
        if (isSet)
        {
            _featureFlags |= (ulong)1 << bitPosition;
        }
        else
        {
            _featureFlags &= ~((ulong)1 << bitPosition);
        }
    }

    /// <summary>
    /// Checks if a feature is set.
    /// </summary>
    /// <param name="feature">Feature to check.</param>
    /// <param name="isCompulsory">If the feature is compulsory.</param>
    /// <returns>true if the feature is set, false otherwise.</returns>
    public bool IsFeatureSet(Feature feature, bool isCompulsory)
    {
        var bitPosition = (int)feature;

        // If the feature is compulsory, adjust the bit position to be even
        if (isCompulsory)
        {
            bitPosition--;
        }

        return IsFeatureSet(bitPosition);
    }
    /// <summary>
    /// Checks if a feature is set.
    /// </summary>
    /// <param name="bitPosition">The bit position of the feature to check.</param>
    /// <param name="isCompulsory">If the feature is compulsory.</param>
    /// <returns>true if the feature is set, false otherwise.</returns>
    public bool IsFeatureSet(int bitPosition, bool isCompulsory)
    {
        // If the feature is compulsory, adjust the bit position to be even
        if (isCompulsory)
        {
            bitPosition--;
        }

        return (_featureFlags & ((ulong)1 << bitPosition)) != 0;
    }
    /// <summary>
    /// Checks if a feature is set.
    /// </summary>
    /// <param name="bitPosition">The bit position of the feature to check.</param>
    /// <returns>true if the feature is set, false otherwise.</returns>
    public bool IsFeatureSet(int bitPosition)
    {
        return (_featureFlags & ((ulong)1 << bitPosition)) != 0;
    }

    /// <summary>
    /// Checks if the option_anchor_outputs or option_anchors_zero_fee_htlc_tx feature is set.
    /// </summary>
    /// <returns>true if one of the features are set, false otherwise.</returns>
    public bool IsOptionAnchorsSet()
    {
        return IsFeatureSet(Feature.OptionAnchorOutputs, false) || IsFeatureSet(Feature.OptionAnchorsZeroFeeHtlcTx, false);
    }

    /// <summary>
    /// Check if this feature set is compatible with the other provided feature set.
    /// </summary>
    /// <param name="other">The other feature set to check compatibility with.</param>
    /// <returns>true if the feature sets are compatible, false otherwise.</returns>
    /// <remarks>
    /// The other feature set must support the var_onion_optin feature.
    /// The other feature set must have all dependencies set.
    /// </remarks>
    public bool IsCompatible(Features other)
    {
        // Check if the other node supports var_onion_optin
        if (!other.IsFeatureSet(Feature.VarOnionOptin, false) && !other.IsFeatureSet(Feature.VarOnionOptin, true))
        {
            return false;
        }

        for (var i = 1; i < sizeof(ulong) * 8; i += 2)
        {
            var isLocalOptionalSet = IsFeatureSet(i, false);
            var isLocalCompulsorySet = IsFeatureSet(i, true);
            var isOtherOptionalSet = other.IsFeatureSet(i, false);
            var isOtherCompulsorySet = other.IsFeatureSet(i, true);

            // If feature is unknown
            if (!Enum.IsDefined(typeof(Feature), i))
            {
                // If the feature is unknown and even, close the connection
                if (isOtherCompulsorySet)
                {
                    return false;
                }
            }
            else
            {
                // If the local feature is compulsory, the other feature should also be set (either optional or compulsory)
                if (isLocalCompulsorySet && !(isOtherOptionalSet || isOtherCompulsorySet))
                {
                    return false;
                }

                // If the other feature is compulsory, the local feature should also be set (either optional or compulsory)
                if (isOtherCompulsorySet && !(isLocalOptionalSet || isLocalCompulsorySet))
                {
                    return false;
                }
            }
        }

        // Check if all of the other node's dependencies are set
        if (!other.AreDependenciesSet())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Serializes the features to a binary writer.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <param name="asGlobal">If the features should be serialized as a global feature set.</param>
    /// <param name="includeLength">If the length of the byte array should be included.</param>
    /// <remarks>
    /// If the features are serialized as a global feature set, only the first 13 bits are serialized.
    /// </remarks>
    /// <remarks>
    /// If the length of the byte array is included, the first 2 bytes are written as the length of the byte array.
    /// </remarks>
    public async Task SerializeAsync(Stream stream, bool asGlobal = false, bool includeLength = true)
    {
        // If it's a global feature, cut out any bit greater than 13
        if (asGlobal)
        {
            _featureFlags &= 0x1FFF;
        }

        // Convert ulong to byte array
        var bytes = EndianBitConverter.GetBytesBE(_featureFlags, includeLength);

        // Write the length of the byte array or 1 if all bytes are zero
        if (includeLength)
        {
            await stream.WriteAsync(EndianBitConverter.GetBytesBE((ushort)bytes.Length));
        }

        // Otherwise, return the array starting from the first non-zero byte
        await stream.WriteAsync(bytes);
    }

    /// <summary>
    /// Deserializes the features from a binary reader.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <param name="includeLength">If the length of the byte array is included.</param>
    /// <remarks>
    /// If the length of the byte array is included, the first 2 bytes are read as the length of the byte array.
    /// </remarks>
    public static async Task<Features> DeserializeAsync(Stream stream, bool includeLength = true)
    {
        var length = 8;

        var bytes = new byte[2];
        if (includeLength)
        {
            // Read the length of the byte array
            await stream.ReadExactlyAsync(bytes);
            length = EndianBitConverter.ToUInt16BE(bytes);
        }

        // Read the byte array
        bytes = new byte[length];
        await stream.ReadExactlyAsync(bytes);

        // Convert the byte array to ulong
        return new()
        {
            _featureFlags = EndianBitConverter.ToUInt64BE(bytes, length < 8)
        };
    }

    /// <summary>
    /// Combines two feature sets.
    /// </summary>
    /// <param name="first">The first feature set.</param>
    /// <param name="second">The second feature set.</param>
    /// <returns>The combined feature set.</returns>
    /// <remarks>
    /// The combined feature set is the logical OR of the two feature sets.
    /// </remarks>
    public static Features Combine(Features first, Features second)
    {
        // Combine (logical OR) the two feature bitmaps into one logical features map
        return new Features
        {
            _featureFlags = first._featureFlags | second._featureFlags
        };
    }

    /// <summary>
    /// Checks if a feature is set.
    /// </summary>
    /// <param name="feature">The feature to check.</param>
    /// <returns>true if the feature is set, false otherwise.</returns>
    /// <remarks>
    /// We don't care if the feature is compulsory or optional.
    /// </remarks>
    public bool HasFeature(Feature feature)
    {
        // Check if feature is either set as compulsory or optional
        return IsFeatureSet(feature, false) || IsFeatureSet(feature, true);
    }

    /// <summary>
    /// Checks if all dependencies are set.
    /// </summary>
    /// <returns>true if all dependencies are set, false otherwise.</returns>
    /// <remarks>
    private bool AreDependenciesSet()
    {
        // Check if all known (Feature Enum) dependencies are set if the feature is set
        foreach (var feature in Enum.GetValues<Feature>())
        {
            if (IsFeatureSet((int)feature, false) || IsFeatureSet((int)feature, true))
            {
                if (s_featureDependencies.TryGetValue(feature, out var dependencies))
                {
                    foreach (var dependency in dependencies)
                    {
                        if (!IsFeatureSet(dependency, false) && !IsFeatureSet(dependency, true))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
}