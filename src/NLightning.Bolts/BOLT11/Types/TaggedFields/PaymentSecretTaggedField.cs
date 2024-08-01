using System.Diagnostics.CodeAnalysis;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;
using NBitcoin;

/// <summary>
/// Tagged field for the payment secret
/// </summary>
/// <remarks>
/// The payment secret is a 32 byte secret that is used to identify a payment
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="uint256"/>
public sealed class PaymentSecretTaggedField : BaseTaggedField<uint256>
{
    /// <summary>
    /// Constructor for PaymentSecretTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PaymentSecretTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PaymentSecretTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.PaymentSecret, bitReader, length)
    { }

    /// <summary>
    /// Constructor for PaymentSecretTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PaymentSecretTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="uint256"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PaymentSecretTaggedField(uint256 value) : base(TaggedFieldTypes.PaymentSecret, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Data.Length == 32;
    }

    /// <inheritdoc/>
    /// <returns>The payment secret as a uint256</returns>
    protected override uint256 Decode(byte[] data)
    {
        return new uint256(data);
    }

    /// <inheritdoc/>
    /// <returns>The payment secret as a byte array</returns>
    protected override byte[] Encode(uint256 value)
    {
        return value.ToBytes();
    }
}