using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

/// <summary>
/// Tagged field for the expiry time
/// </summary>
/// <remarks>
/// The expiry time is the time in seconds after which the invoice is invalid.
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
public sealed class ExpiryTimeTaggedField : BaseTaggedField<int>
{
    /// <summary>
    /// Constructor for ExpiryTimeTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a ExpiryTimeTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public ExpiryTimeTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.ExpiryTime)
    {
        Value = bitReader.ReadInt32FromBits(length * 5);
        Data = Encode(Value);
    }

    /// <summary>
    /// Constructor for ExpiryTimeTaggedField from a value
    /// </summary>
    /// <param name="value">The expiration in seconds</param>
    /// <remarks>
    /// This constructor is used to create a ExpiryTimeTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public ExpiryTimeTaggedField(int value) : base(TaggedFieldTypes.ExpiryTime, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Value >= 0;
    }

    /// <inheritdoc/>
    /// <returns>The expiry time in seconds</returns>
    protected override int Decode(byte[] data)
    {
        return EndianBitConverter.ToInt32BE(data, true);
    }

    /// <inheritdoc/>
    /// <returns>The expiry time as a byte array</returns>
    protected override byte[] Encode(int value)
    {
        return EndianBitConverter.GetBytesBE(value, true);
    }
}