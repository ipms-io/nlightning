using NBitcoin;

namespace NLightning.Bolt11.Models.TaggedFields;

using Constants;
using Domain.Utils;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the payee public key
/// </summary>
/// <remarks>
/// The payee public key is a 33-byte public key used to identify the payee
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class PayeePubKeyTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.PayeePubKey;
    internal PubKey Value { get; }
    public short Length => TaggedFieldConstants.PayeePubkeyLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiryTimeTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    internal PayeePubKeyTaggedField(PubKey value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        bitWriter.WriteBits(Value.ToBytes(), Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Reads a PayeePubKeyTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The PayeePubKeyTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static PayeePubKeyTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length != TaggedFieldConstants.PayeePubkeyLength)
            throw new ArgumentException(
                $"Invalid length for {nameof(DescriptionHashTaggedField)}. Expected {TaggedFieldConstants.PayeePubkeyLength}, but got {length}");

        // Read the data from the BitReader
        var data = new byte[33];
        bitReader.ReadBits(data, length * 5);

        return new PayeePubKeyTaggedField(new PubKey(data));
    }
}