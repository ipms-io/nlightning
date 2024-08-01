using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Enums;

/// <summary>
/// Tagged field for the payee public key
/// </summary>
/// <remarks>
/// The payee public key is a 33 byte public key that is used to identify the payee
/// </remarks>
/// <seealso cref="BaseTaggedField{T}"/>
/// <seealso cref="TaggedFieldTypes"/>
/// <seealso cref="PubKey"/>
public sealed class PayeePubKeyTaggedField : BaseTaggedField<PubKey>
{
    /// <summary>
    /// Constructor for PayeePubKeyTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="buffer">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PayeePubKeyTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PayeePubKeyTaggedField(BitReader buffer, short length) : base(TaggedFieldTypes.PayeePubKey, buffer, length)
    { }

    /// <summary>
    /// Constructor for PayeePubKeyTaggedField from a value
    /// </summary>
    /// <param name="value">The value of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a PayeePubKeyTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="PubKey"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public PayeePubKeyTaggedField(PubKey value) : base(TaggedFieldTypes.PayeePubKey, value)
    { }

    /// <inheritdoc/>
    public override bool IsValid()
    {
        return Data.Length == 33;
    }

    /// <inheritdoc/>
    /// <returns>The payee public key as a PubKey</returns>
    protected override PubKey Decode(byte[] data)
    {
        return new PubKey(data);
    }

    /// <inheritdoc/>
    /// <returns>The payee public key as a PubKey</returns>
    protected override byte[] Encode(PubKey value)
    {
        return value.ToBytes();
    }
}