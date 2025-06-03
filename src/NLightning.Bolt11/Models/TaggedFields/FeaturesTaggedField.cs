using NLightning.Domain.Utils;

namespace NLightning.Bolt11.Models.TaggedFields;

using Domain.Node;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for features
/// </summary>
/// <remarks>
/// The features are a collection of features that are supported by the node.
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class FeaturesTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.Features;
    internal FeatureSet Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description</param>
    internal FeaturesTaggedField(FeatureSet value)
    {
        Value = value;
        Length = (short)(value.SizeInBits / 5 + (value.SizeInBits % 5 == 0 ? 0 : 1));
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        var shouldPad = Length * 5 / 8 == (Length * 5 - 7) / 8;

        // Write data
        Value.WriteToBitWriter(bitWriter, Length * 5, shouldPad);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Reads a FeaturesTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The FeaturesTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static FeaturesTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for FeaturesTaggedField. Length must be greater than 0", nameof(length));
        }

        var shouldPad = length * 5 / 8 == (length * 5 - 7) / 8;
        var features = FeatureSet.DeserializeFromBitReader(bitReader, length * 5, shouldPad);

        return new FeaturesTaggedField(features);
    }
}