using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for the metadata
/// </summary>
/// <remarks>
/// The metadata is a variable length field that can be used to store additional information
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
public sealed class MetadataTaggedField : BaseTaggedField<byte[]>
{
    /// <summary>
    /// Constructor for MetadataTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a MetadataTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public MetadataTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.Metadata, bitReader, length)
    { }

    /// <summary>
    /// Constructor for MetadataTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a MetadataTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public MetadataTaggedField(byte[] value) : base(TaggedFieldTypes.Metadata, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value != null;
    }

    /// <inheritdoc/>
    /// <returns>The metadata as a byte array</returns>
    protected override byte[] Decode(byte[] data)
    {
        return data;
    }

    /// <inheritdoc/>
    /// <returns>The metadata as a byte array</returns>
    protected override byte[] Encode(byte[] value)
    {
        return AccountForPaddingWhenEncoding(value);
    }

}