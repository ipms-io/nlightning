using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for the description hash
/// </summary>
/// <remarks>
/// The description hash is a 32 byte hash that is used to identify a description
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="uint256"/>
public sealed class DescriptionHashTaggedField : BaseTaggedField<uint256>
{
    /// <summary>
    /// Constructor for DescriptionHashTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a DescriptionHashTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public DescriptionHashTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.DescriptionHash, bitReader, length)
    { }

    /// <summary>
    /// Constructor for DescriptionHashTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a DescriptionHashTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="uint256"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public DescriptionHashTaggedField(uint256 value) : base(TaggedFieldTypes.DescriptionHash, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value != uint256.Zero;
    }

    /// <inheritdoc/>
    /// <returns>The description hash as a uint256</returns>
    /// <seealso cref="uint256"/>
    protected override uint256 Decode(byte[] data)
    {
        return new uint256(data);
    }

    /// <inheritdoc/>
    /// <returns>The description hash as a uint256</returns>
    /// <seealso cref="uint256"/>
    protected override byte[] Encode(uint256 value)
    {
        return AccountForPaddingWhenEncoding(value.ToBytes());
    }
}