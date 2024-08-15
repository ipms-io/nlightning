namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT9;
using Common.BitUtils;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for features
/// </summary>
/// <remarks>
/// The features are a collection of features that are supported by the node.
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class FeaturesTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.FEATURES;
    public Features Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description</param>
    public FeaturesTaggedField(Features value)
    {
        Value = value;
        Length = (short)(value.SizeInBits / 5 + (value.SizeInBits % 5 == 0 ? 0 : 1));
    }

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

    public object GetValue()
    {
        return Value;
    }

    public static FeaturesTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for FeaturesTaggedField. Length must be greater than 0", nameof(length));
        }

        var shouldPad = length * 5 / 8 == (length * 5 - 7) / 8;
        var features = Features.DeserializeFromBitReader(bitReader, length * 5, shouldPad);

        return new FeaturesTaggedField(features);
    }
}