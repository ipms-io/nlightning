namespace NLightning.Bolts.BOLT9;

public class Features
{
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

    private readonly bool _isGlobal;

    private ulong _featureFlags;

    public Features(bool isGlobal = false)
    {
        _isGlobal = isGlobal;

        // Allways set the compulsory bit of initial_routing_sync
        SetFeature(Feature.InitialRoutingSync, true);

        // Allways set the compulsory bit of var_onion_optin
        SetFeature(Feature.VarOnionOptin, false);
    }

    public void SetFeature(Feature feature, bool isCompulsory, bool isSet = true)
    {
        // We cannot set features greater than 13 if they are global
        if (_isGlobal && (int)feature > 13)
        {
            throw new ArgumentException("Global features cannot be set for features greater than 13.");
        }

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

    public bool IsFeatureSet(Feature feature, bool isCompulsory)
    {
        // If the feature is global the maximum bit position is 13
        if (_isGlobal && (int)feature > 13)
        {
            return false;
        }

        var bitPosition = (int)feature;

        // If the feature is compulsory, adjust the bit position to be even
        if (isCompulsory)
        {
            bitPosition--;
        }

        return IsFeatureSet(bitPosition);
    }

    public bool IsOptionAnchorsSet()
    {
        return IsFeatureSet(Feature.OptionAnchorOutputs, false) || IsFeatureSet(Feature.OptionAnchorsZeroFeeHtlcTx, false);
    }

    public bool IsCompatible(Features other)
    {
        // Check if both the optional and the compulsory feature bits in a pair are set
        foreach (Feature feature in Enum.GetValues(typeof(Feature)))
        {
            // Skip the check for the compulsory bit of initial_routing_sync
            if (feature == Feature.InitialRoutingSync || feature == Feature.VarOnionOptin)
            {
                continue;
            }

            var isLocalOptionalSet = IsFeatureSet(feature, false);
            var isLocalCompulsorySet = IsFeatureSet(feature, true);
            var isOtherOptionalSet = other.IsFeatureSet(feature, false);
            var isOtherCompulsorySet = other.IsFeatureSet(feature, true);

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

        if (!(IsFeatureSet(Feature.VarOnionOptin, false) || IsFeatureSet(Feature.VarOnionOptin, true)) ||
            !(other.IsFeatureSet(Feature.VarOnionOptin, false) || other.IsFeatureSet(Feature.VarOnionOptin, true)))
        {
            return false;
        }

        return true;
    }

    public void Serialize(BinaryWriter writer, bool includeLength = true)
    {
        // Convert ulong to byte array
        var bytes = EndianBitConverter.GetBytesBE(_featureFlags, includeLength);

        // Write the length of the byte array or 1 if all bytes are zero
        if (includeLength)
        {
            writer.Write(EndianBitConverter.GetBytesBE((ushort)bytes.Length));
        }

        // Otherwise, return the array starting from the first non-zero byte
        writer.Write(bytes);
    }

    public void Deserialize(BinaryReader reader, bool includeLength = true)
    {
        var length = 8;

        if (includeLength)
        {
            // Read the length of the byte array
            length = EndianBitConverter.ToUInt16BE(reader.ReadBytes(2));
        }

        // Read the byte array
        var bytes = reader.ReadBytes(length);

        // Convert the byte array to ulong
        _featureFlags = EndianBitConverter.ToUInt64BE(bytes, length < 8);
    }

    private void SetFeature(int bitPosition, bool isSet)
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

    private bool IsFeatureSet(int bitPosition)
    {
        return (_featureFlags & ((ulong)1 << bitPosition)) != 0;
    }
}