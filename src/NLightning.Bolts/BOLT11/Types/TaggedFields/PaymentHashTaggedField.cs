using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

/// <summary>
/// Tagged field for the payment hash
/// </summary>
/// <remarks>
/// The payment hash is a 32 byte hash that is used to identify a payment
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="uint256"/>
public sealed class PaymentHashTaggedField : BaseTaggedField<uint256>
{
    /// <summary>
    /// Constructor for PaymentHashTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PaymentHashTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PaymentHashTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.PaymentHash, bitReader, length)
    { }

    /// <summary>
    /// Constructor for PaymentHashTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PaymentHashTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="uint256"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PaymentHashTaggedField(uint256 value) : base(TaggedFieldTypes.PaymentHash, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Data.Length == 32;
    }

    /// <inheritdoc/>
    /// <returns>The payment hash as a uint256</returns>
    /// <seealso cref="uint256"/>
    protected override uint256 Decode(byte[] data)
    {
        return new uint256(data);
    }

    protected override byte[] Encode(uint256 value)
    {
        return value.ToBytes();
    }
}