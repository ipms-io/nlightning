using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using NLightning.Common.Extensions;

namespace NLightning.Bolts.BOLT9;

using Common.BitUtils;

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
        { Feature.GOSSIP_QUERIES_EX,            new[] { Feature.GOSSIP_QUERIES } },
        { Feature.PAYMENT_SECRET,              new[] { Feature.VAR_ONION_OPTIN } },
        { Feature.BASIC_MPP,                   new[] { Feature.PAYMENT_SECRET } },
        { Feature.OPTION_ANCHOR_OUTPUTS,        new[] { Feature.OPTION_STATIC_REMOTE_KEY } },
        { Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, new[] { Feature.OPTION_STATIC_REMOTE_KEY } },
        { Feature.OPTION_ROUTE_BLINDING,        new[] { Feature.VAR_ONION_OPTIN } },
        { Feature.OPTION_ZEROCONF,             new[] { Feature.OPTION_SCID_ALIAS } },
    };

    private BitArray _featureFlags;

    /// <summary>
    /// Initializes a new instance of the <see cref="Features"/> class.
    /// </summary>
    /// <remarks>
    /// Always set the bit of <see cref="Feature.VAR_ONION_OPTIN"/> as Optional.
    /// </remarks>
    public Features()
    {
        _featureFlags = new BitArray(128);
        // Always set the compulsory bit of var_onion_optin
        SetFeature(Feature.VAR_ONION_OPTIN, false);
    }

    public int SizeInBits => _featureFlags.GetLastIndexOfOne();

    /// <summary>
    /// Sets a feature.
    /// </summary>
    /// <param name="feature">The feature to set.</param>
    /// <param name="isCompulsory">If the feature is compulsory.</param>
    /// <param name="isSet">true to set the feature, false to unset it</param>
    /// <remarks>
    /// If the feature has dependencies, they will be set first.
    /// The dependencies keep the same isCompulsory value as the feature being set.
    /// </remarks>
    public void SetFeature(Feature feature, bool isCompulsory, bool isSet = true)
    {
        // If we're setting the feature, and it has dependencies, set them first
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
        else // If we're unsetting the feature, and it has dependents, unset them first
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
        if (bitPosition >= _featureFlags.Length)
        {
            _featureFlags.Length = bitPosition + 1;
        }

        _featureFlags.Set(bitPosition, isSet);
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

        return IsFeatureSet(bitPosition);
    }
    /// <summary>
    /// Checks if a feature is set.
    /// </summary>
    /// <param name="bitPosition">The bit position of the feature to check.</param>
    /// <returns>true if the feature is set, false otherwise.</returns>
    private bool IsFeatureSet(int bitPosition)
    {
        if (bitPosition >= _featureFlags.Length)
        {
            return false;
        }

        return _featureFlags.Get(bitPosition);
    }

    /// <summary>
    /// Checks if the option_anchor_outputs or option_anchors_zero_fee_htlc_tx feature is set.
    /// </summary>
    /// <returns>true if one of the features are set, false otherwise.</returns>
    public bool IsOptionAnchorsSet()
    {
        return IsFeatureSet(Feature.OPTION_ANCHOR_OUTPUTS, false) || IsFeatureSet(Feature.OPTION_ANCHORS_ZERO_FEE_HTLC_TX, false);
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
        if (!other.IsFeatureSet(Feature.VAR_ONION_OPTIN, false) && !other.IsFeatureSet(Feature.VAR_ONION_OPTIN, true))
        {
            return false;
        }

        for (var i = 1; i < _featureFlags.Length; i += 2)
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

        // Check if all the other node's dependencies are set
        return other.AreDependenciesSet();
    }

    /// <summary>
    /// Serializes the features to a binary writer.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
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
            _featureFlags.Length = 13;
        }

        // Convert BitArray to byte array
        var bytes = new byte[(_featureFlags.Length + 7) / 8];
        _featureFlags.CopyTo(bytes, 0);

        // Set bytes as big endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        // Trim leading zero bytes
        var leadingZeroBytes = 0;
        foreach (var t in bytes)
        {
            if (t == 0)
            {
                leadingZeroBytes++;
            }
            else
            {
                break;
            }
        }

        var trimmedBytes = bytes[leadingZeroBytes..];

        // Write the length of the byte array or 1 if all bytes are zero
        if (includeLength)
        {
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)trimmedBytes.Length));
        }

        // Otherwise, return the array starting from the first non-zero byte
        await stream.WriteAsync(trimmedBytes);
    }

    /// <summary>
    /// Serializes the features to a byte array.
    /// </summary>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        var length = SizeInBits;
        var shouldPad = length * 5 / 8 == (length * 5 - 7) / 8;

        // Write bits in reverse order
        for (var i = length; i >= (shouldPad ? 0 : 1) && bitWriter.HasMoreBits(1); i--)
        {
            bitWriter.WriteBit(_featureFlags[i]);
        }
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
    /// Deserializes the features from a binary reader.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="includeLength">If the length of the byte array is included.</param>
    /// <remarks>
    /// If the length of the byte array is included, the first 2 bytes are read as the length of the byte array.
    /// </remarks>
    /// <returns>The deserialized features.</returns>
    /// <exception cref="SerializationException">Error deserializing Features</exception>
    public static async Task<Features> DeserializeAsync(Stream stream, bool includeLength = true)
    {
        try
        {
            var length = 8;

            var bytes = new byte[2];
            if (includeLength)
            {
                // Read the length of the byte array
                await stream.ReadExactlyAsync(bytes);
                length = EndianBitConverter.ToUInt16BigEndian(bytes);
            }

            // Read the byte array
            bytes = new byte[length];
            await stream.ReadExactlyAsync(bytes);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            // Convert the byte array to BitArray
            return new Features { _featureFlags = new BitArray(bytes) };
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing Features", e);
        }
    }

    /// <summary>
    /// Deserializes the features from a byte array.
    /// </summary>
    /// <param name="data">The byte array to deserialize from.</param>
    /// <remarks>
    /// The byte array can have a length less than or equal to 8 bytes.
    /// </remarks>
    /// <returns>The deserialized features.</returns>
    /// <exception cref="SerializationException">Error deserializing Features</exception>
    public static Features DeserializeFromBytes(byte[] data)
    {
        try
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }

            var bitArray = new BitArray(data);
            return new Features { _featureFlags = bitArray };
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing Features", e);
        }
    }

    /// <summary>
    /// Deserializes the features from a BitReader.
    /// </summary>
    /// <param name="bitReader">The bit reader to read from.</param>
    /// <param name="length">The number of bits to read.</param>
    /// <param name="shouldPad">If the bit array should be padded.</param>
    /// <returns>The deserialized features.</returns>
    /// <exception cref="SerializationException">Error deserializing Features</exception>
    public static Features DeserializeFromBitReader(BitReader bitReader, int length, bool shouldPad)
    {
        try
        {
            // Create a new bit array
            var bitArray = new BitArray(length + (shouldPad ? 1 : 0));
            for (var i = 0; i < length; i++)
            {
                bitArray.Set(length - i - (shouldPad ? 0 : 1), bitReader.ReadBit());
            }
            // 100000000000000000000000000000000000000000000000000000000000000000000000000100000100000000
            return new Features { _featureFlags = bitArray };
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing Features", e);
        }
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
        var combinedLength = Math.Max(first._featureFlags.Length, second._featureFlags.Length);
        var combinedFlags = new BitArray(combinedLength);

        for (var i = 0; i < combinedLength; i++)
        {
            combinedFlags.Set(i, first.IsFeatureSet(i) || second.IsFeatureSet(i));
        }

        return new Features { _featureFlags = combinedFlags };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < _featureFlags.Length; i++)
        {
            if (IsFeatureSet(i))
            {
                sb.Append($"{(Feature)i}, ");
            }
        }

        return sb.ToString().TrimEnd(' ', ',');
    }

    /// <summary>
    /// Checks if all dependencies are set.
    /// </summary>
    /// <returns>true if all dependencies are set, false otherwise.</returns>
    /// <remarks>
    /// This method is used to check if all dependencies are set when a feature is set.
    /// </remarks>
    private bool AreDependenciesSet()
    {
        // Check if all known (Feature Enum) dependencies are set if the feature is set
        foreach (var feature in Enum.GetValues<Feature>())
        {
            if (!IsFeatureSet((int)feature, false) && !IsFeatureSet((int)feature, true))
            {
                continue;
            }

            if (!s_featureDependencies.TryGetValue(feature, out var dependencies))
            {
                continue;
            }

            if (dependencies.Any(dependency => !IsFeatureSet(dependency, false) && !IsFeatureSet(dependency, true)))
            {
                return false;
            }
        }

        return true;
    }
}