using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for the minimum final cltv expiry
/// </summary>
/// <remarks>
/// The minimum final cltv expiry is a 4 byte field that specifies the minimum number of blocks that the receiver should wait to claim the payment
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
public sealed class MinFinalCltvExpiryTaggedField : BaseTaggedField<ushort>
{
    /// <summary>
    /// Constructor for MinFinalCltvExpiryTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a MinFinalCltvExpiryTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public MinFinalCltvExpiryTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.ExpiryTime, bitReader, length)
    { }

    /// <summary>
    /// Constructor for MinFinalCltvExpiryTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a MinFinalCltvExpiryTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public MinFinalCltvExpiryTaggedField(ushort value) : base(TaggedFieldTypes.ExpiryTime, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value > 0;
    }

    /// <inheritdoc/>
    /// <returns>The minimum final cltv expiry as an int</returns>
    protected override ushort Decode(byte[] data)
    {
        return EndianBitConverter.ToUInt16BE(data, true);
    }

    /// <inheritdoc/>
    /// <returns>The minimum final cltv expiry as a byte array</returns>
    protected override byte[] Encode(ushort value)
    {
        return AccountForPaddingWhenEncoding(EndianBitConverter.GetBytesBE(value, true));
    }
}