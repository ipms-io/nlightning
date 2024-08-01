using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;
using BOLT9;

/// <summary>
/// Tagged field for features
/// </summary>
/// <remarks>
/// The features are a collection of features that are supported by the node.
/// </remarks>
/// <seealso cref="Features"/>
/// <seealso cref="BaseTaggedField{T}"/>
public sealed class FeaturesTaggedField : BaseTaggedField<Features>
{
    /// <summary>
    /// Constructor for FeaturesTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a FeaturesTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public FeaturesTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.Features)
    {
        Value = Features.DeserializeFromBitReader(bitReader, (short)(length * 5));
        Data = Encode(Value);
    }

    /// <summary>
    /// Constructor for FeaturesTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a FeaturesTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="Features"/>
    /// <seealso cref="Feature"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public FeaturesTaggedField(Features value) : base(TaggedFieldTypes.Features, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value != null;
    }

    /// <inheritdoc/>
    /// <returns>The features as a Features object</returns>
    /// <seealso cref="Features"/>
    /// <seealso cref="Feature"/>
    protected override Features Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    /// <returns>The features as a byte array</returns>
    /// <seealso cref="Features"/>
    /// <seealso cref="Feature"/>
    protected override byte[] Encode(Features value)
    {
        return value.SerializeToBits();
    }
}