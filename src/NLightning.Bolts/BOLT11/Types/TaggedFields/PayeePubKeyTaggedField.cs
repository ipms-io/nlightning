using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Constants;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the payee public key
/// </summary>
/// <remarks>
/// The payee public key is a 33 byte public key that is used to identify the payee
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class PayeePubKeyTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.PAYEE_PUB_KEY;
    public PubKey Value { get; }
    public short Length => TaggedFieldConstants.PAYEE_PUBKEY_LENGTH;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiryTimeTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    public PayeePubKeyTaggedField(PubKey value)
    {
        Value = value;
    }

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

    public object GetValue()
    {
        return Value;
    }

    public static PayeePubKeyTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length != TaggedFieldConstants.PAYEE_PUBKEY_LENGTH)
        {
            throw new ArgumentException($"Invalid length for DescriptionHashTaggedField. Expected {TaggedFieldConstants.PAYEE_PUBKEY_LENGTH}, but got {length}");
        }

        // Read the data from the BitReader
        var data = new byte[(TaggedFieldConstants.PAYEE_PUBKEY_LENGTH * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);

        return new PayeePubKeyTaggedField(new PubKey(data));
    }
}